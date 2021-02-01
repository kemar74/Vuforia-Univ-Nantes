using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hideable : MonoBehaviour
{
    public enum HIDE_TYPE {HIDE_TOP, HIDE_BOTTOM, HIDE_LEFT, HIDE_RIGHT};
    public HIDE_TYPE hide_type;
    public bool hidden = false;
    public bool Hidden {get {return hidden;} set {setHidden(value);} }
    public float speed = 1.0f;
    private Vector3 hide_pos;
    private Vector3 show_pos;
    private Vector3 translation_hide_to_show;
    private RectTransform my_rect;
    // Start is called before the first frame update
    void Start()
    {
        my_rect = GetComponent<RectTransform>();
        show_pos = transform.position;
        switch (hide_type) {
            case HIDE_TYPE.HIDE_BOTTOM:
                hide_pos = show_pos - new Vector3(0, my_rect.rect.height);
                break;
            case HIDE_TYPE.HIDE_TOP:
                hide_pos = show_pos + new Vector3(0, my_rect.rect.height);
                break;
            case HIDE_TYPE.HIDE_LEFT:
                hide_pos = show_pos - new Vector3(my_rect.rect.width, 0);
                break;
            case HIDE_TYPE.HIDE_RIGHT:
                hide_pos = show_pos + new Vector3(my_rect.rect.width, 0);
                break;
        }
        translation_hide_to_show = show_pos - hide_pos;

        if (Hidden) {
            transform.position = hide_pos; // Hide it at first if it should start hidden
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hidden) {
            if ((transform.position - hide_pos).magnitude > 10) {
                transform.Translate(-translation_hide_to_show * (Time.deltaTime / speed));
            }
        } else {
            if ((transform.position - show_pos).magnitude > 10) {
                transform.Translate(translation_hide_to_show * (Time.deltaTime / speed));
            }
        }
    }

    public void setHidden(bool _hidden) {
        if (_hidden == true) {
            Hide();
        } else {
            Show();
        }
    }
    public void Show() {
        hidden = false;
    }
    public void Hide() {
        hidden = true;
    }
}
