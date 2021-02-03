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
    private List<GameObject> selection {get {return history[historyIndex];} set {updateHistory(value);}}//= new List<GameObject>(); // All the points that are selected

    private List<List<GameObject>> history = new List<List<GameObject>>(); // History of selections made to undo or redo
    private int historyIndex = -1;

    public Button btnUndo; // Button to undo last selection
    public Button btnRedo; // Button to redo last selection
    public Button btnIncompleteShapeError; // Button to click if the program thinks you did an incomplete shape on selection but you want to keep it
    public CanvasRenderer incompleteShapeErrorContainer; // The container with the error message, so that we can hide it when we dont need it
    public GameObject linePrefab; 
    public GameObject currentLine;

    public LineRenderer lineRendered;
    // Start is called before the first frame update
    void Start()
    {
        selection = new List<GameObject>();
        // Initialize all the variables
        currentSelectionPath = new List<Vector2>();
        camera = GameObject.FindObjectsOfType<Camera>()[0]; // We take the camera from the scene. It's not the best way to do, but it works
        pointsPos = new List<Tuple<GameObject, Vector2>>();
        PlotParent = plotParent;

        btnSelect.interactable = false; // Disable the select button as selection is the default behaviour
        btnDeselect.interactable = true; // Enable the deselection button
        btnSelect.onClick.AddListener(delegate{setSelecting(true);}); // When one of the button is clicked,
        btnDeselect.onClick.AddListener(delegate{setSelecting(false);}); // we change the value of "isSelected"

        if (btnUndo != null) { // If a button is specified for undo
            btnUndo.onClick.AddListener(undo); // Add the action if it is clicked
            btnUndo.transform.parent.gameObject.GetComponent<Hideable>().Hide(); // Hide the buttons at start
        }
        if (btnRedo != null) { // If a button is specified for redo
            btnRedo.onClick.AddListener(redo); // Add the action if it is clicked
            btnRedo.transform.parent.gameObject.GetComponent<Hideable>().Hide(); // Hide the buttons at start
        }

        if (btnIncompleteShapeError != null) {
            btnIncompleteShapeError.onClick.AddListener(redo); // For now, we just cancel the "incomplete shape" error
            btnIncompleteShapeError.onClick.AddListener(hideIncompleteShapeMessage);

        }
        
        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        //currentLine.transform.parent = btnSelect.transform;
        lineRendered = currentLine.GetComponent<LineRenderer>();
        lineRendered.startColor = Color.white;
        lineRendered.endColor = Color.white;
        Material whiteDiffuseMat = new Material(Shader.Find("Unlit/Texture"));
        lineRendered.material = whiteDiffuseMat;
    }

    // Change the isSelecting value and enable/disable the buttons
    protected void setSelecting(bool newVal) {
        isSelecting = newVal;
        btnSelect.interactable = !newVal;
        btnDeselect.interactable = newVal;
    }

    // Function to get the position of each points on the screen
    private void recalculatePointPos() {
        pointsPos.Clear();
        if (plotParent != null) {
            for (int i = 0; i < plotParent.transform.childCount; i++) { // We have tuples <point, position>
                if(plotParent.transform.GetChild(i).gameObject.GetComponent<PlottedBalls>() != null) {
                    pointsPos.Add(new Tuple<GameObject, Vector2>(plotParent.transform.GetChild(i).gameObject, camera.WorldToScreenPoint(plotParent.transform.GetChild(i).gameObject.transform.position)));
                }
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
        if (currentSelectionPath.Count > 0) {
            DisplayMaybeSelectedPoints();
        }
        // If smartphone user
        foreach(Touch touch in Input.touches) { // One touch = One finger
            if(touch.phase == TouchPhase.Began) { // If it's the begining of a new path, empty the path
                hideIncompleteShapeMessage();
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
            hideIncompleteShapeMessage();
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

        DisplaySelectionPath();
    }

    // During the draw of the user, highlight points that are in the area
    protected void DisplayMaybeSelectedPoints() {
        recalculatePointPos();
        foreach(Tuple<GameObject, Vector2> elem in pointsPos) {
            if (checkPointInsidePolygon(currentSelectionPath, elem.Item2)) { // If it's in the region, change the color
                elem.Item1.GetComponent<PlottedBalls>().MaybeSelected = true;
            } else {
                elem.Item1.GetComponent<PlottedBalls>().MaybeSelected = false; // Otherwise, use normal color
            }
        }
    }

    // public void OnDrawGizmos() { // Not working yet
    //     print("Gizmos");
    //     DisplaySelectionPath();
    // }
    // public void OnPostRender() {
    //     print("PostRender");
    //     DisplaySelectionPath();
    // }

    protected void selectPoints() {
        List<GameObject> currentSelection = new List<GameObject>(selection);
        recalculatePointPos(); // Calculate the position of all the points
        if (currentSelectionPath.Count <= 2) { // Select one single point if there is only one stroke
            // We only look at the first point as the two positions should be very close
            Ray ray = camera.ScreenPointToRay(new Vector3(currentSelectionPath[0].x, currentSelectionPath[0].y));
            RaycastHit hit;
            // The raycast is a virtual line that is drawn and that checks if it collides with a GameObject
            if (Physics.Raycast(ray, out hit)) { // If it hit something
                if(hit.collider.gameObject.GetComponent<PlottedBalls>() != null) { // Check if the object is really a point
                    // Either we change the selection of this point
                    if (hit.collider.gameObject.GetComponent<PlottedBalls>().Selected == true) {
                        currentSelection.Remove(hit.collider.gameObject);
                    } else {
                        currentSelection.Add(hit.collider.gameObject);
                    }

                    // Or we select/deselect depending on our mode
                    /*
                    if (isSelecting) {
                        currentSelection.Add(hit.collider.gameObject); // We add this object to the selection
                    } else {
                        currentSelection.Remove(hit.collider.gameObject); // Or we remove it
                        hit.collider.gameObject.GetComponent<PlottedBalls>().Selected = false; // We set the state of the ball to "deselected"
                    }
                    */
                }
            }
        } else { // Select region
            List<GameObject> points = new List<GameObject>();
            for(int i = 0; i < pointsPos.Count; i++) {
                points.Add(pointsPos[i].Item1);
            }
            if (isSelecting) {
                currentSelection.AddRange(getPointsInPolygon(points, this.currentSelectionPath)); // Check all the points in the region
                // and add them to the selection
            } else {
                foreach(GameObject go in getPointsInPolygon(points, this.currentSelectionPath)) {
                    currentSelection.Remove(go); // Same process, but to remove them
                }
            }
        }
        if (selection.Count != currentSelection.Count) {
            selection = removeDuplicateSelections(currentSelection); // We don't want duplicates in the list of selected points
            updateSelectionDeselection();
            if(!isShapeClosed(currentSelectionPath)) {
                proposeIncompleteShapeSelection();
            }
        }

        currentSelectionPath.Clear(); // Free the path
        DisplayMaybeSelectedPoints(); // This will just remove the "maybe selected" state to the points
    }

    public void updateSelectionDeselection() {
        for(int i = 0; i < pointsPos.Count; i++) {
            if (selection.Contains(pointsPos[i].Item1)) {
                pointsPos[i].Item1.GetComponent<PlottedBalls>().Selected = true;
            } else {
                pointsPos[i].Item1.GetComponent<PlottedBalls>().Selected = false;
            }
        }
        UpdateGUI(); // Display statistics of the selected points
    }

    // For each point in the plot, check if it is in the region and return all the contained points
    protected List<GameObject> getPointsInPolygon(List<GameObject> plot, List<Vector2> polygon) {
        List<GameObject> containedPoints = new List<GameObject>(); // Start with an empty list
        for(int i = 0; i < plot.Count; i++) {   // Check each point is contained
            if (plot[i].GetComponent<PlottedBalls>() != null &&  // Check if it's really a point
                checkPointInsidePolygon(polygon, camera.WorldToScreenPoint(plot[i].transform.position))) {
                containedPoints.Add(plot[i]); // If yes, add them to the list of contained points
                plot[i].GetComponent<PlottedBalls>().Selected = isSelecting; // This has to be changed later...
            }
        }
        return containedPoints; // Return the contained points
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
            GUI_Text_Container.gameObject.GetComponent<Hideable>().Hide();
        } else {
            GUI_Text_Container.gameObject.GetComponent<Hideable>().Show();
        }
    }

    // Display the path the user is actually drawing (not working yet)
    public void DisplaySelectionPath() {
        // line.transform.position.Set(0, 0, GameObject.Find("BackgroundPlane").transform.position.z - 0.5f);
        // print(GameObject.Find("BackgroundPlane").transform.position.z);
        // line.GetComponent<LineRenderer>().positionCount = currentSelectionPath.Count;
        // List<Vector3> vec3pos = new List<Vector3>();
        // foreach(Vector2 pos in currentSelectionPath) {
        //     vec3pos.Add((Vector3) pos);
        // }
        // line.GetComponent<LineRenderer>().SetPositions(vec3pos.ToArray());
        
        // Vector3 pos = camera.ScreenToViewportPoint(Input.mousePosition);
        Vector2 diffScreenGUI = new Vector2(Screen.width/2, Screen.height/2);
        lineRendered.positionCount = currentSelectionPath.Count ;
        if (lineRendered.positionCount > 0) {
            lineRendered.SetPosition(lineRendered.positionCount -1, new Vector3( currentSelectionPath[currentSelectionPath.Count - 1].x - diffScreenGUI.x,  currentSelectionPath[currentSelectionPath.Count - 1].y - diffScreenGUI.y, 1200f));                
        // lineRendered.SetPosition(lineRendered.positionCount -1, pos); //new Vector3( currentSelectionPath[currentSelectionPath.Count - 1].x - diffScreenGUI.x,  currentSelectionPath[currentSelectionPath.Count - 1].y - diffScreenGUI.y, 1024f));        
        }        
    }

    // Remove duplicate points in the selected list
    private List<GameObject> removeDuplicateSelections(List<GameObject> sel) {
        return new List<GameObject>(new HashSet<GameObject>(sel));
    }

    // We want to change an element in the history, and prevent "redo"
    private void updateHistory(List<GameObject> newSelection) {
        historyIndex++; // Increase the history index
        if (historyIndex >= history.Count) { // If it's a new element, add it
            history.Add(newSelection);
        } else {
            history[historyIndex] = newSelection; // Otherwise modify the list
            history.RemoveRange(historyIndex+1, history.Count - historyIndex - 1); // And remove future to avoid redos
        }
        updateUndoRedoButtons();
    }

    public void undo() {
        if (historyIndex > 0) {
            historyIndex--;
        }
        updateSelectionDeselection();
        updateUndoRedoButtons();
    }
    public void redo() {
        if (historyIndex < history.Count - 1) {
            historyIndex++;
        }
        updateSelectionDeselection();
        updateUndoRedoButtons();
    }

    private void updateUndoRedoButtons() {
        bool hideButtons = true;
        if (btnUndo != null) {
            btnUndo.interactable = historyIndex > 0;
            hideButtons &= btnUndo.interactable == false;
        }
        if (btnRedo != null) {
            btnRedo.interactable = historyIndex < history.Count-1;
            hideButtons &= btnRedo.interactable == false;
        }
        btnUndo.transform.parent.GetComponent<Hideable>().Hidden = hideButtons;
    }

    // Check if a path looks like a close region.
    // Condition used:
    // If the last point is closer than Ri * D, it's closed
    // With D = max distance from the start to any other point
    // and Ri = Incomplete Rate a value between 0 (start and end must be the same point) and 1 (no verification)
    private bool isShapeClosed(List<Vector2> polygon) {
        float incompleteRate = 5/6.0f;

        float maxDistanceToStart = 0.0f;
        for (int i = 0; i < polygon.Count; i++) {
            maxDistanceToStart = Mathf.Max(maxDistanceToStart, (polygon[0] - polygon[i]).magnitude);
        }
        float distanceFromFirstToLast = (polygon[0] - polygon[polygon.Count - 1]).magnitude;

        if (distanceFromFirstToLast > incompleteRate * maxDistanceToStart) {
            return false;
        } else {
            return true;
        }
    }

    // Display a message explaining that the selection shape drawn is not closed enough
    public void proposeIncompleteShapeSelection() {
        undo();
        if (incompleteShapeErrorContainer != null) {
            incompleteShapeErrorContainer.GetComponent<Hideable>().Show();
        }
    }
    // Hiding the message saying the selection shape is not closed enough
    public void hideIncompleteShapeMessage() {
        if (incompleteShapeErrorContainer != null) {
            incompleteShapeErrorContainer.GetComponent<Hideable>().Hide();
        }
    }
}
