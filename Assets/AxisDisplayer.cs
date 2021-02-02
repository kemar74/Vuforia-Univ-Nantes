using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxisDisplayer : MonoBehaviour
{
    protected GameObject Xaxis;
    protected GameObject Yaxis;
    protected GameObject Zaxis;
    protected GameObject Xlabel;
    protected GameObject Ylabel;
    protected GameObject Zlabel;
    void Start()
    {
        Xaxis = transform.Find("Xaxis").gameObject;
        Yaxis = transform.Find("Yaxis").gameObject;
        Zaxis = transform.Find("Zaxis").gameObject;
        Xlabel = transform.Find("Xlabel").gameObject;
        Ylabel = transform.Find("Ylabel").gameObject;
        Zlabel = transform.Find("Zlabel").gameObject;

        Xaxis.GetComponent<Renderer>().material.color = Color.red;
        Yaxis.GetComponent<Renderer>().material.color = Color.green;
        Zaxis.GetComponent<Renderer>().material.color = Color.blue;
    }

    public void changeAxisLabels(string xlabel, string ylabel, string zlabel) {
        Xlabel.GetComponent<TextMesh>().text = xlabel;
        Ylabel.GetComponent<TextMesh>().text = ylabel;
        Zlabel.GetComponent<TextMesh>().text = zlabel;
    }
}
