using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Component used to add more behaviour to a point in a plot
public class PlottedBalls : MonoBehaviour
{
    protected bool selected = false; // is the point selected by the user?
    public bool Selected {get {return selected;} set {setSelected(value);} }
    private MeshRenderer mesh; // The mesh used (a sphere)

    public Color selectedColor = Color.HSVToRGB(0.0f, 1.0f, 1.0f); // One color if the point is selected
    public Color deselectedColor = Color.HSVToRGB(0.0f, 0.5f, 1.0f); // A color if it is deselected
    public Color maybeSelectedColor = Color.HSVToRGB(0.15f, 1.0f, 1.0f); // Color if the user is drawing a path around this point

    public Dictionary<string, object> Data {get; set; } // Data about this point

    void Awake() {
        Data = new Dictionary<string, object>();
    }
    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
        mesh.material.color = deselectedColor; // This point is deselected by default

    }

    // When we change the selection of this point, change its color
    void setSelected(bool newSelected) {
        selected = newSelected;
        if (selected) {
            mesh.material.color = selectedColor;
        } else {
            mesh.material.color = deselectedColor;
        }
    }

    // Adding data to the data object
    public void setData(string key, object value) {
        if (Data.ContainsKey(key)) {
            Data[key] = value;
        } else {
            Data.Add(key, value);
        }
    }
}
