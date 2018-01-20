using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour {

    public int  SceneID;
    public string TagPerson;

    void OnTriggerStay(Collider other)
    {
        if (other.tag == TagPerson) {
            if (Input.GetKey(KeyCode.E)){
                SceneManager.LoadScene(SceneID);
            }
        }
    }
}
