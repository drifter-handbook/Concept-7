using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// exempts objects spawned from an attachment from colliding with their attached object.
public class ActorExemptCollisionAttach : MonoBehaviour, IActorAttachment, IActorSpawnHandler
{
    StageActor target;

    public void Attach(StageActor target, StageActor source)
    {
        this.target = target;
        ActorCollisionCaller.SetExempt(gameObject, target.gameObject);
    }

    public void HandleSpawn(StageActor newActor)
    {
        ActorCollisionCaller caller = GetComponent<ActorCollisionCaller>();
        // exempt new actor from colliding with parent
        if (target != null && caller != null)
        {
            // copy exemptions to spawned child
            foreach (GameObject go in caller.Exempt)
            {
                ActorCollisionCaller.SetExempt(go, newActor.gameObject);
            }
        }
    }
}
