using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchInput : MonoBehaviour
{
    public Button btnSelect;
    public Button btnDeselect;
    public GameObject plotParent;
    //public Camera camera;

    public GameObject PlotParent {get {return plotParent;} set {setPlotParent(value);} }

    private bool isSelecting = true;
    public bool IsSelecting {get {return isSelecting;} set {setSelecting(value);} }

    private List<Tuple<GameObject, Vector2>> pointsPos;
    new public Camera camera;

    private List<Vector2> path;
    public Text GUI_Text;
    public CanvasRenderer GUI_Text_Container;
    private List<GameObject> selection = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        path = new List<Vector2>();
        camera = GameObject.FindObjectsOfType<Camera>()[0];
        pointsPos = new List<Tuple<GameObject, Vector2>>();
        PlotParent = plotParent;

        btnSelect.interactable = false;
        btnDeselect.interactable = true;
        btnSelect.onClick.AddListener(delegate{setSelecting(true);});
        btnDeselect.onClick.AddListener(delegate{setSelecting(false);});
    }

    protected void setSelecting(bool newVal) {
        isSelecting = newVal;
        btnSelect.interactable = !newVal;
        btnDeselect.interactable = newVal;
    }
    private void recalculatePointPos() {
        if (plotParent != null) {
            for (int i = 0; i < plotParent.transform.childCount; i++) {
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
        foreach(Touch touch in Input.touches) {
            if(touch.phase == TouchPhase.Began) {
                path.Clear();
            }
            if (path.Count > 0) {
                if(new Vector2(Input.mousePosition.x, Input.mousePosition.y) != path[path.Count - 1]) {
                    path.Add(touch.position);
                }
            } else {
                path.Add(touch.position);
            }
            if (touch.phase == TouchPhase.Ended) {
                selectPoints();
            }
        }

        // PC user
        if (Input.GetMouseButtonDown(0)) {
            path.Clear();
        }
        if (Input.GetMouseButton(0)) {
            if (path.Count > 0) {
                if(new Vector2(Input.mousePosition.x, Input.mousePosition.y) != path[path.Count - 1]) {
                    path.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
                }
            } else {
                path.Add(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            selectPoints();
        }
    }

    protected void selectPoints() {
        recalculatePointPos();
        if (path.Count <= 2) { // Select one single point
            // We only look at the first point as the two positions should be very close
            Ray ray = camera.ScreenPointToRay(new Vector3(path[0].x, path[0].y));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit)) { // If it hit something
                if (isSelecting) {
                    selection.Add(hit.collider.gameObject);
                } else {
                    selection.Remove(hit.collider.gameObject);
                    hit.collider.gameObject.GetComponent<PlottedBalls>().Selected = false;
                }
            }
        } else { // Select area
            List<GameObject> points = new List<GameObject>();
            for(int i = 0; i < pointsPos.Count; i++) {
                points.Add(pointsPos[i].Item1);
            }
            if (isSelecting) {
                selection.AddRange(getPointsInPolygon(points, this.path));
            } else {
                foreach(GameObject go in getPointsInPolygon(points, this.path)) {
                    selection.Remove(go);
                }
            }
        }
        removeDuplicateSelections();

        for(int i = 0; i < selection.Count; i++) {
            selection[i].GetComponent<PlottedBalls>().Selected = true;
        }

        UpdateGUI();
    }
    protected List<GameObject> getPointsInPolygon(List<GameObject> plot, List<Vector2> polygon) {
        List<GameObject> selectedPoints = new List<GameObject>();
        for(int i = 0; i < plot.Count; i++) {
            if (checkPointInsidePolygon(polygon, camera.WorldToScreenPoint(plot[i].transform.position))) {
                selectedPoints.Add(plot[i]);
                plot[i].GetComponent<PlottedBalls>().Selected = isSelecting;
            }
        }
        return selectedPoints;
    }

    private bool checkPointInsidePolygon(List<Vector2> polygon, Vector2 point) {
        Vector2 p1;
        Vector2 p2;

        float sumAngle = 0f;
        for(int i = 0; i < polygon.Count; i++) {
            p1 = polygon[i] - point;
            p2 = polygon[(i + 1) % polygon.Count] - point;
            sumAngle += Vector2.SignedAngle(p1, p2);
        }
        return Math.Abs(sumAngle) >= 180;
    }

    public void UpdateGUI() {
        string text = "";
        if (selection.Count == 1) {
            text = "1 object selected:";
            PlottedBalls uniqueObject = selection[0].GetComponent<PlottedBalls>();
            Dictionary<string, object>.KeyCollection keys = uniqueObject.Data.Keys;
            foreach (string key in keys) {
                text += "\n" + key + ": " + uniqueObject.Data[key];
            }
        } else if (selection.Count > 1) {
            text = selection.Count + " objects selected:";
            text += Test.getDataFromBallsAsText(selection);

        } else {
            text = "No object selected";
        }
        
        GUI_Text.text = text;

        if (selection.Count == 0) {
            GUI_Text_Container.gameObject.SetActive(false);
        } else {
            GUI_Text_Container.gameObject.SetActive(true);
        }
    }

    private void removeDuplicateSelections() {
        List<GameObject> noDuplicate = new List<GameObject>(new HashSet<GameObject>(selection));
        selection.Clear();
        selection.AddRange(noDuplicate);
    }
}
