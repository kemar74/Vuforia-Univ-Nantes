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
                hide_pos = show_pos - new Vector3(0, my_rect.rect.height)*2;
                break;
            case HIDE_TYPE.HIDE_TOP:
                hide_pos = show_pos + new Vector3(0, my_rect.rect.height)*2;
                break;
            case HIDE_TYPE.HIDE_LEFT:
                hide_pos = show_pos - new Vector3(my_rect.rect.width, 0)*2;
                break;
            case HIDE_TYPE.HIDE_RIGHT:
                hide_pos = show_pos + new Vector3(my_rect.rect.width, 0)*2;
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
            if ((transform.position - hide_pos).sqrMagnitude > (translation_hide_to_show * Time.deltaTime/speed).sqrMagnitude) {
                transform.Translate(-translation_hide_to_show * (Time.deltaTime / speed));
            }
        } else {
            if ((transform.position - show_pos).sqrMagnitude > (translation_hide_to_show * Time.deltaTime/speed).sqrMagnitude) {
                transform.Translate(translation_hide_to_show * (Time.deltaTime / speed));
            }
        }
        // bool mustHide = false;
        // bool mustShow = false;
        // if (hidden) {
        //     switch (hide_type) {
        //         case HIDE_TYPE.HIDE_BOTTOM:
        //             mustHide = transform.position.y + my_rect.rect.height > 0;
        //             print(transform.position.y + " " + Screen.height);
        //             break;
        //         case HIDE_TYPE.HIDE_TOP:
        //             mustHide = transform.position.y - my_rect.rect.height < Screen.height;
        //             break;
        //         case HIDE_TYPE.HIDE_LEFT:
        //             mustHide = transform.position.x + my_rect.rect.width > 0;
        //             break;
        //         case HIDE_TYPE.HIDE_RIGHT:
        //             mustHide = transform.position.x - my_rect.rect.width < Screen.width;
        //             break;
        //     }
        // } else {
        //     switch (hide_type) {
        //         case HIDE_TYPE.HIDE_BOTTOM:
        //             mustShow = transform.position.y + my_rect.rect.height > 0;
        //             break;
        //         case HIDE_TYPE.HIDE_TOP:
        //             mustShow = transform.position.y + my_rect.rect.height < Screen.height;
        //             break;
        //         case HIDE_TYPE.HIDE_LEFT:
        //             mustShow = transform.position.x > 0;
        //             break;
        //         case HIDE_TYPE.HIDE_RIGHT:
        //             mustShow = transform.position.x < Screen.width;
        //             break;
        //     }
        // }
        // if (mustHide) {
        //     transform.Translate(-translation_hide_to_show * (Time.deltaTime / speed));
        // } else if (mustShow) {
        //     transform.Translate(translation_hide_to_show * (Time.deltaTime / speed));
        // }
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
