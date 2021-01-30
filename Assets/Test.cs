using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{

    // name of the input file (CSV or JSON file)
    public string inputfile;

    // list for holding data from CSV reader
    private List<Dictionary<string, object>> pointList;


    // Indices for columns to be assigned [try 1,2,3 to have a better scatter plot]
    public int columnX = 1;
    public int columnY = 2;
    public int columnZ = 3;
    
    // Full column names
    public string xName;
    public string yName;
    public string zName;

    // Scale of the scatter plot
    public float plotScale = 10;

    public GameObject PointPrefab;

    public GameObject PointHolder;

    // Start is called before the first frame update
    void Start()
    {
        pointList = CSVReader.Read(inputfile);

        // Debug.Log(pointList);

        // Declare list of strings, fill with keys (column names)
        List<string> columnList = new List<string>(pointList[1].Keys);
        
        // Print number of keys (using .count)
        // Debug.Log("There are " + columnList.Count + " columns in the CSV");
        
        // foreach (string key in columnList)
        //     Debug.Log("Column name is " + key);
        
        // Assign column name from columnList to Name variables
        xName = columnList[columnX];
        yName = columnList[columnY];
        zName = columnList[columnZ];

        //instantiate only one prefab
        //Instantiate(PointPrefab, new Vector3(0,0,0), Quaternion.identity);
        

        // Get maxes of each axis
        float xMax = FindMaxValue(pointList, xName);
        float yMax = FindMaxValue(pointList, yName);
        float zMax = FindMaxValue(pointList, zName);
        
        // Get minimums of each axis
        float xMin = FindMinValue(pointList, xName);
        float yMin = FindMinValue(pointList, yName);
        float zMin = FindMinValue(pointList, zName);

        // Loop through Pointlist
        for (var i = 0; i < pointList.Count; i++)
        {

            float x = 0.0f;
            float y = 0.0f;
            float z = 0.0f;


            //Debug variable types
            //Debug.Log("x value as type " + (pointList[i][xName]).GetType().Name + " value "+ pointList[i][xName]);
            //Debug.Log("y value as type " + (pointList[i][yName]).GetType().Name + " value "+ pointList[i][yName]);
            //Debug.Log("z value as type " + (pointList[i][zName]).GetType().Name + " value "+ pointList[i][zName]);
            

            string v1 = pointList[i][xName].ToString().Replace(".", ",");
            string v2 = pointList[i][yName].ToString().Replace(".", ",");
            string v3 = pointList[i][zName].ToString().Replace(".", ",");


            // Get value in poinList at ith "row", in "column" Name
            
            // OLD VALUE => CAST PROBLEM
            //x = System.Convert.ToSingle(pointList[i][xName]);
            //y = System.Convert.ToSingle(pointList[i][yName]);
            //z = System.Convert.ToSingle(pointList[i][zName]);

            // CORRECT VALUES
            // x = System.Convert.ToSingle(v1);
            // y = System.Convert.ToSingle(v2);
            // z = System.Convert.ToSingle(v3);

            // Get value in poinList at ith "row", in "column" Name, normalize
            x = (System.Convert.ToSingle(v1) - xMin) / (xMax - xMin);
            y = (System.Convert.ToSingle(v2) - yMin) / (yMax - yMin);
            z = (System.Convert.ToSingle(v3) - zMin) / (zMax - zMin);

            // Debug cast values
            //Debug.Log("x = " + x + " y = "+y+" z = "+z);
            //Debug.Log("x type = " + x.GetType().Name + " y type = "+y.GetType().Name+" z type = "+z.GetType().Name);
        
            
            //instantiate the prefab with coordinates defined above
            //Instantiate(PointPrefab, new Vector3(x, y, z), Quaternion.identity);

            // Instantiate as gameobject variable so that it can be manipulated within loop
            //GameObject dataPoint = Instantiate(PointPrefab, new Vector3(x, y, z), Quaternion.identity);
            GameObject dataPoint = Instantiate(PointPrefab, new Vector3(x, y, z) * plotScale, Quaternion.identity, PointHolder.transform);
            dataPoint.transform.localScale = new Vector3(0.10f * plotScale, 0.10f * plotScale, 0.10f * plotScale);

            // Make dataPoint child of PointHolder object 
            // dataPoint.transform.parent = PointHolder.transform;

            // Assigns original values to dataPointName
            string dataPointName = pointList[i][xName].ToString() + " "+ pointList[i][yName].ToString() + " "+ pointList[i][zName].ToString();

            // Assigns name to the prefab
            dataPoint.transform.name = dataPointName;

            // Gets material color and sets it to a new RGBA color we define
            dataPoint.GetComponent<Renderer>().material.color = new Color(x,y,z, 1.0f);
    
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }


    // Normalization functions : get minimum value + maximum value of a column

    private float FindMaxValue(List<Dictionary<string, object>> obj, string columnName)
    {
        //set initial value to first value
        //float maxValue = Convert.ToSingle(pointList[0][columnName]);

        string value = obj[0][columnName].ToString().Replace(".", ",");
        float maxValue = System.Convert.ToSingle(value);

        float f_value = 0.0f;

        //Loop through Dictionary, overwrite existing maxValue if new value is larger
        for (var i = 0; i < obj.Count; i++)
        {
            value = obj[i][columnName].ToString().Replace(".", ",");
            f_value = System.Convert.ToSingle(value);
            if (maxValue < f_value )
                maxValue = f_value;
        }
    
        //Spit out the max value
        return maxValue;
    }


    private float FindMinValue(List<Dictionary<string, object>> obj, string columnName)
   {
 
        //set initial value to first value
        //float minValue = Convert.ToSingle(pointList[0][columnName]);

        string value = obj[0][columnName].ToString().Replace(".", ",");
        float minValue = System.Convert.ToSingle(value);

        float f_value = 0.0f;

        //Loop through Dictionary, overwrite existing minValue if new value is smaller
        for (var i = 0; i < obj.Count; i++)
        {
            value = obj[i][columnName].ToString().Replace(".", ",");
            f_value = System.Convert.ToSingle(value);

            if (f_value < minValue)
                minValue = f_value;
        }

        return minValue;
   }

}
