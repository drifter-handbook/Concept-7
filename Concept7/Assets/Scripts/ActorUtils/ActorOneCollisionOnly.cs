using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorOneCollisionOnly : MonoBehaviour, IActorCollisionHandler
{
    public int Order => 100;

    public void HandleCollision(GameObject other)
    {
        if (gameObject.tag == "PlayArea")
        {
            return;
        }
        StageActor target = other.GetComponent<StageActor>();
        if (target != null)
        {
            Destroy(gameObject);
        }
    }
}
