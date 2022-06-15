using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorDestroyOnImpact : MonoBehaviour, IActorCollisionHandler
{
    public int Order => 2;

    public void HandleCollision(GameObject other)
    {
        if (other.tag == "PlayArea")
        {
            return;
        }
        ActorSuppressOtherDestroyOnImpact suppress = other.GetComponent<ActorSuppressOtherDestroyOnImpact>();
        StageActor actor = GetComponent<StageActor>();
        if (suppress == null || actor == null || !suppress.Classifications.Contains(actor.Classification))
        {
            Destroy(gameObject);
        }
    }
}
