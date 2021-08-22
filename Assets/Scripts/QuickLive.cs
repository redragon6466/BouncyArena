using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickLive : MonoBehaviour
{
    private float timeToLive;
    private float timeAlive;
    // Start is called before the first frame update
    void Start()
    {
        timeToLive = 5;
        timeAlive = 0;
    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if(timeAlive>timeToLive)
        {
            Destroy(this.gameObject);
        }
    }
}
