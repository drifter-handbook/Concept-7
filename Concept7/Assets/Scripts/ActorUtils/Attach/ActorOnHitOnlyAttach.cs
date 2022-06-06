using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorOnHitOnlyAttach : MonoBehaviour, IActorAttachment
{
    StageActor target;

    void Start()
    {
        StartCoroutine(DestroyInOneFrame());
    }

    public void Attach(StageActor target, StageActor source)
    {
        this.target = target;
    }

    IEnumerator DestroyInOneFrame()
    {
        yield return null;
        yield return null;
        // if target is still alive
        if (target != null)
        {
            foreach (var handler in gameObject.GetComponentsInChildren<IActorDestroyHandler>())
            {
                handler.HandleDestroy(ActorDestroyReason.Event);
            }
        }
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
