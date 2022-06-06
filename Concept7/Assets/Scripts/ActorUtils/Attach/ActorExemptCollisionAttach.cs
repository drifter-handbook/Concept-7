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
    }

    public void HandleSpawn(StageActor newActor)
    {
        // exempt new actor from colliding with parent
        if (target != null)
        {
            Exempt(newActor.gameObject, target.gameObject);
        }
    }

    void Exempt(GameObject a, GameObject b)
    {
        // exempt a from colliding with b
        var imp = a.GetComponent<ActorDestroyOnImpact>();
        if (imp != null)
        {
            imp.Exempt.Add(b);
        }
        var hp = a.GetComponent<ActorUseHP>();
        if (hp != null)
        {
            hp.Exempt.Add(b);
        }
        // exempt b from colliding with a
        imp = b.GetComponent<ActorDestroyOnImpact>();
        if (imp != null)
        {
            imp.Exempt.Add(a);
        }
        hp = b.GetComponent<ActorUseHP>();
        if (hp != null)
        {
            hp.Exempt.Add(a);
        }
    }
}
