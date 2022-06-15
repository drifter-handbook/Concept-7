using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyOffscreen : MonoBehaviour
{
    void Update()
    {
        if (!PlayArea.WithinBounds(transform.position))
        {
            Cleanup();
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag != "PlayArea")
        {
            return;
        }
        Cleanup();
    }

    void Cleanup()
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
