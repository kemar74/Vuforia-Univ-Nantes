using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchInput : MonoBehaviour
{
    public Button btnSelect; // Button to enable the selection
    public Button btnDeselect; // Button to enable deselection
    public GameObject plotParent; // The plot that is displayed
    public GameObject PlotParent {get {return plotParent;} set {setPlotParent(value);} }

    private bool isSelecting = true; // Variable to track if we are selecting or deselecting
    public bool IsSelecting {get {return isSelecting;} set {setSelecting(value);} }

    private List<Tuple<GameObject, Vector2>> pointsPos; // Position of each point of the plot in the screen
    new public Camera camera; // Main camera

    private List<Vector2> currentSelectionPath; // Path drawn by the finger to select points
    public Text GUI_Text; // Text that shows statistics
    public CanvasRenderer GUI_Text_Container; // The container of the text, so we can hide it when nothing has to be shown
    private List<GameObject> selection = new List<GameObject>(); // All the points that are selected

    // Start is called before the first frame update
    void Start()
    {
        // Initialize all the variables
        currentSelectionPath = new List<Vector2>();
        camera = GameObject.FindObjectsOfType<Camera>()[0]; // We take the camera from the scene. It's not the best way to do, but it works
        pointsPos = new List<Tuple<GameObject, Vector2>>();
        PlotParent = plotParent;

        btnSelect.interactable = false; // Disable the select button as selection is the default behaviour
        btnDeselect.interactable = true; // Enable the deselection button
        btnSelect.onClick.AddListener(delegate{setSelecting(true);}); // When one of the button is clicked,
        btnDeselect.onClick.AddListener(delegate{setSelecting(false);}); // we change the value of "isSelected"
    }

    // Change the isSelecting value and enable/disable the buttons
    protected void setSelecting(bool newVal) {
        isSelecting = newVal;
        btnSelect.interactable = !newVal;
        btnDeselect.interactable = newVal;
    }

    // Function to get the position of each points on the screen
    private void recalculatePointPos() {
        if (plotParent != null) {
            for (int i = 0; i < plotParent.transform.childCount; i++) { // We have tuples <point, position>
                pointsPos.Add(new Tuple<GameObject, Vector2>(plotParent.transform.GetChild(i).gameObject, camera.WorldToScreenPoint(plotParent.transform.GetChild(i).gameObject.transform.position)));
            }
        }
    }
    public void setPlotParent(GameObject newPlot) {
        plotParent = newPlot;
        recalculatePointPos();
    }
    
    // Update is called once per frame
    void Update()
    {
        // If smartphone user
        foreach(Touch touch in Input.touches) { // One touch = One finger
            if(touch.phase == TouchPhase.Began) { // If it's the begining of a new path, empty the path
                currentSelectionPath.Clear();
            }
            if (currentSelectionPath.Count > 0) { // Store the new position if the finger is not static
                if(new Vector2(Input.mousePosition.x, Input.mousePosition.y) != currentSelectionPath[currentSelectionPath.Count - 1]) {
                    currentSelectionPath.Add(touch.position);
                }
            } else { // If it's the first point, add it to the path
                currentSelectionPath.Add(touch.position);
            }
            if (touch.phase == TouchPhase.Ended) { // When the finger is released, do the selection process
                selectPoints();
            }
        }

        // PC user (exactly the same process as the smartphone users)
        if (Input.GetMouseButtonDown(0)) {
            currentSelectionPath.Clear();
        }
        if (Input.GetMouseButton(0)) {
            if (currentSelectionPath.Count > 0) {
                if(new Vector2(Input.mousePosition.x, Input.mousePosition.y) != currentSelectionPath[currentSelectionPath.Count - 1]) {
                    currentSelectionPath.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                }
            } else {
                currentSelectionPath.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            selectPoints();
        }
    }

    // public void OnDrawGizmos() { // Not working yet
    //     DisplaySelectionPath();
    // }

    protected void selectPoints() {
        recalculatePointPos(); // Calculate the position of all the points
        if (currentSelectionPath.Count <= 2) { // Select one single point if there is only one stroke
            // We only look at the first point as the two positions should be very close
            Ray ray = camera.ScreenPointToRay(new Vector3(currentSelectionPath[0].x, currentSelectionPath[0].y));
            RaycastHit hit;
            // The raycast is a virtual line that is drawn and that checks if it collides with a GameObject
            if (Physics.Raycast(ray, out hit)) { // If it hit something
                if (isSelecting) {
                    selection.Add(hit.collider.gameObject); // We add this object to the selection
                } else {
                    selection.Remove(hit.collider.gameObject); // Or we remove it
                    hit.collider.gameObject.GetComponent<PlottedBalls>().Selected = false; // We set the state of the ball to "deselected"
                }
            }
        } else { // Select region
            List<GameObject> points = new List<GameObject>();
            for(int i = 0; i < pointsPos.Count; i++) {
                points.Add(pointsPos[i].Item1);
            }
            if (isSelecting) {
                selection.AddRange(getPointsInPolygon(points, this.currentSelectionPath)); // Check all the points in the region
                // and add them to the selection
            } else {
                foreach(GameObject go in getPointsInPolygon(points, this.currentSelectionPath)) {
                    selection.Remove(go); // Same process, but to remove them
                }
            }
        }
        removeDuplicateSelections(); // We don't want duplicates in the list of selected points

        for(int i = 0; i < selection.Count; i++) {
            selection[i].GetComponent<PlottedBalls>().Selected = true; // Set the selected objects state to "selected"
        }
        UpdateGUI(); // Display statistics of the selected points
    }

    // For each point in the plot, check if it is in the region and return all the contained points
    protected List<GameObject> getPointsInPolygon(List<GameObject> plot, List<Vector2> polygon) {
        List<GameObject> selectedPoints = new List<GameObject>(); // Start with an empty list
        for(int i = 0; i < plot.Count; i++) {   // Check each point is contained
            if (checkPointInsidePolygon(polygon, camera.WorldToScreenPoint(plot[i].transform.position))) {
                selectedPoints.Add(plot[i]); // If yes, add them to the list of contained points
                plot[i].GetComponent<PlottedBalls>().Selected = isSelecting; // This has to be changed later...
            }
        }
        return selectedPoints; // Return the contained points
    }

    // Check if a point is contained in a polygon
    private bool checkPointInsidePolygon(List<Vector2> polygon, Vector2 point) {
        Vector2 p1;
        Vector2 p2;

        float sumAngle = 0f; // Track the total angle of all the edges of the polygon and the point
        for(int i = 0; i < polygon.Count; i++) {
            p1 = polygon[i] - point;
            p2 = polygon[(i + 1) % polygon.Count] - point;
            sumAngle += Vector2.SignedAngle(p1, p2);
        }
        return Math.Abs(sumAngle) >= 180; // If the angle is lower than 180, it's outside
    }

    // Update the text that displays the statistics of the selected points
    // This function shouldn't be in this class, but I don't give a shit
    public void UpdateGUI() {
        string text = "";
        if (selection.Count == 1) { // If there is only one point
            text = "1 object selected:";
            PlottedBalls uniqueObject = selection[0].GetComponent<PlottedBalls>(); // Properties are stored in PlottedBalls component
            Dictionary<string, object>.KeyCollection keys = uniqueObject.Data.Keys;
            foreach (string key in keys) {
                text += "\n" + key + ": " + uniqueObject.Data[key]; // Display each property
            }
        } else if (selection.Count > 1) { // If there are multiple points selected
            text = selection.Count + " objects selected:";
            text += Plotter.getDataFromBallsAsText(selection); // Use the static function of the plotting class (we should change the name...)

        } else {
            text = "No object selected"; // If there is nothing selected, we will hide this
        }

        GUI_Text.text = text; // Set the displayed text

        if (selection.Count == 0) { // Remove the text completely if there is nothing to display
            GUI_Text_Container.gameObject.SetActive(false);
        } else {
            GUI_Text_Container.gameObject.SetActive(true);
        }
    }

    // Display the path the user is actually drawing (not working yet)
    protected void DisplaySelectionPath() {
        Gizmos.color = Color.white;
        for(int i = 0; i < currentSelectionPath.Count - 1; i++) {
            Gizmos.DrawLine(currentSelectionPath[i], currentSelectionPath[i+1]);
        }
    }

    // Remove duplicate points in the selected list
    private void removeDuplicateSelections() {
        List<GameObject> noDuplicate = new List<GameObject>(new HashSet<GameObject>(selection));
        selection.Clear();
        selection.AddRange(noDuplicate);
    }
}
