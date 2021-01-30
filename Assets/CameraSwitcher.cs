using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;


public class CameraSwitcher : MonoBehaviour
{
    private VuforiaBehaviour mQCAR;
    private Button button;
    // Start is called before the first frame update
    void Start()
    {
        mQCAR = (VuforiaBehaviour)FindObjectOfType(typeof(VuforiaBehaviour));
        button = GetComponent<Button>();
        button.onClick.AddListener(SwitchCam);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchCam() {
    }
}
