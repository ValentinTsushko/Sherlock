using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour {
    public Canvas MenuElements;
    public Canvas GameElements;
    public Button ButtonClick;
    void Start()
    {
        GameElements.enabled = false;
        var ButtonElement =  ButtonClick.GetComponent<Button>() as Button;
        if( ButtonElement.tag == "Start" ){
            ButtonElement.onClick.AddListener(delegate { StartGameClick(); });
        }
    }

    void StartGameClick() {
        Debug.Log("Click");
        MenuElements.enabled = false;
        GameElements.enabled = true;
    }

}           
