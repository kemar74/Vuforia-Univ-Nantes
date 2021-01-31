using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlottedBalls : MonoBehaviour
{
    protected bool selected = false;
    public bool Selected {get {return selected;} set {setSelected(value);} }
    private MeshRenderer mesh;

    public Color selectedColor = Color.HSVToRGB(0.0f, 1.0f, 1.0f);
    public Color deselectedColor = Color.HSVToRGB(0.0f, 0.5f, 1.0f);

    public Dictionary<string, object> Data {get; set; }

    void Awake() {
        print("HERE");
        Data = new Dictionary<string, object>();
    }
    // Start is called before the first frame update
    void Start()
    {
        mesh = gameObject.GetComponent<MeshRenderer>();
        mesh.material.color = deselectedColor;

    }

    void setSelected(bool newSelected) {
        selected = newSelected;
        if (selected) {
            mesh.material.color = selectedColor;
        } else {
            mesh.material.color = deselectedColor;
        }
    }

    public void setData(string key, object value) {
        if (Data.ContainsKey(key)) {
            Data[key] = value;
        } else {
            Data.Add(key, value);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
