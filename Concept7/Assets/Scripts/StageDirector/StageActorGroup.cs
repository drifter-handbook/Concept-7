using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageActorGroup : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool hasChildren = false;
        foreach (Transform child in transform)
        {
            hasChildren = true;
        }
        if (!hasChildren)
        {
            Destroy(gameObject);
        }
    }
}
