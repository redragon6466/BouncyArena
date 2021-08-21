using Assets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 CamNextLocation;
    public Vector3 CamNextRotationVec;
    public Quaternion CamNextRotation;
 
    void Start()
    {
       
    }

    public void OnLevelWasLoaded()
    {
        transform.position = CamNextLocation;
        transform.rotation = Quaternion.Euler(CamNextRotationVec.x, CamNextRotationVec.y, CamNextRotationVec.z); ;
    }

    void Awake()
    {
        var god = GameObject.Find("God").GetComponent<God>();

        // move the camera
        CamNextLocation = god.SelectedCameraPos;
        CamNextRotationVec = god.SelectedCameraRot;
        OnLevelWasLoaded();
    }

    public void OnLevelWasLoaded(Vector3 pos, Quaternion rot)
    {
        transform.position = pos;
        transform.rotation = rot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
