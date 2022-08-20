using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillAfterSeconds : MonoBehaviour
{
    public bool timerStart = false;
    public float time = 3;
    // Update is called once per frame
    void Update()
    {
        if(timerStart){
            time -= Time.deltaTime;
            if(time <= 0){
                Destroy(this.gameObject);
            }
        }
    }
}
