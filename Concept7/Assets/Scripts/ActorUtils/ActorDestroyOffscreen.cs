using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyOffscreen : MonoBehaviour
{
    void OnTriggerExit2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "PlayArea")
        {
            Destroy(gameObject);
        }
    }
}
