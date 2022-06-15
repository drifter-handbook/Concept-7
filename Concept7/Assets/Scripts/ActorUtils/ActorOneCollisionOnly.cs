using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorOneCollisionOnly : MonoBehaviour, IActorCollisionHandler
{
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
