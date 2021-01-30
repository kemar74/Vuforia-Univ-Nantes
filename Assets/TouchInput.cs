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

    private int maxPointsInPath = 20;

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
        List<GameObject> points = new List<GameObject>();
        for(int i = 0; i < pointsPos.Count; i++) {
            points.Add(pointsPos[i].Item1);
        }
        List<GameObject> selection = getPointsInPolygon(points, this.path);
        for(int i = 0; i < selection.Count; i++) {
            Color col = Color.HSVToRGB((IsSelecting ? 0.0f : 0.5f), 1.0f, 1.0f);
            selection[i].GetComponent<Renderer>().material.color = col;
        }
    }
    protected List<GameObject> getPointsInPolygon(List<GameObject> plot, List<Vector2> polygon) {
        List<GameObject> selectedPoints = new List<GameObject>();
        for(int i = 0; i < plot.Count; i++) {
            if (checkPointInsidePolygon(polygon, camera.WorldToScreenPoint(plot[i].transform.position))) {
                selectedPoints.Add(plot[i]);
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
}
