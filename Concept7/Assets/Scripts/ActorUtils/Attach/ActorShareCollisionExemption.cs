using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// exempts objects spawned from an attachment from colliding with their attached object.
[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorShareCollisionExemption : MonoBehaviour, IActorAttachment, IActorSpawnHandler
{
    StageActor target;

    public void Attach(StageActor target, StageActor source)
    {
        this.target = target;
        if (target != null)
        {
            ActorCollisionCaller.SetExempt(gameObject, target.gameObject);
        }
    }

    public void HandleSpawn(StageActor newActor)
    {
        ActorCollisionCaller caller = GetComponent<ActorCollisionCaller>();
        ActorCollisionCaller newActorCaller = newActor.GetComponent<ActorCollisionCaller>();
        // exempt new actor from colliding with parent
        if (caller != null && newActorCaller != null)
        {
            // copy exemptions to spawned child
            if (newActorCaller.Exempt != null)
            {
                caller.Exempt.UnionWith(newActorCaller.Exempt);
            }
            newActorCaller.Exempt = caller.Exempt;
        }
    }
}
