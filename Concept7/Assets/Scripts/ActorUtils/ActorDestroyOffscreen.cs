using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyOffscreen : MonoBehaviour
{
    void OnTriggerExit2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "PlayArea")
        {
            StageActor actor = GetComponent<StageActor>();
            if (gameObject != null && actor != null)
            {
                foreach (var handler in gameObject.GetComponentsInChildren<IActorDestroyHandler>())
                {
                    handler.HandleDestroy(ActorDestroyReason.Offscreen);
                }
                Destroy(gameObject);
            }
        }
    }
}
