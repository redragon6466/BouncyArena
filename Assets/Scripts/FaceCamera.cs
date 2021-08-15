using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Camera mainCamera;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //set our rotation to zero/zero?
        //get the camera in the scene
        //get it's rotation
        mainCamera = Camera.main;
        if(mainCamera != null)
        {
            transform.rotation = mainCamera.transform.rotation;
            transform.localPosition = new Vector3(0, 0, 0);
            //Debug.Log(mainCamera.transform.rotation);

        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            transform.localPosition = new Vector3(0, 0, 0);
        }
        
    }
    
}
