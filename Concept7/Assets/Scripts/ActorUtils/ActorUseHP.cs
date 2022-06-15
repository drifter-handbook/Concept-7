using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorUseHP : MonoBehaviour, IActorCollisionHandler
{
    public float health;

    public void Initialize(StageData.Actor actor)
    {
        health = actor.Hp ?? 1;
    }

    public void HandleCollision(GameObject other)
    {
        if (other.tag == "PlayArea")
        {
            return;
        }
        StageActor actor = GetComponent<StageActor>();
        if (actor != null)
        {
            ActorSuppressOtherUseHP suppress = other.GetComponent<ActorSuppressOtherUseHP>();
            if (suppress == null || !suppress.Classifications.Contains(actor.Classification))
            {
                if (other.tag == "PlayerWeapon")
                {
                    health -= (float?)other.GetComponent<PlayerWeapon>()?.weaponData.damage ?? 0;
                }
                if (health <= 0)
                {
                    Die();
                }
            }
        }
    }

    void Die()
    {
        //play an animation here maybe?
        StageActor actor = GetComponent<StageActor>();
        if (gameObject != null && actor != null)
        {
            foreach (var handler in gameObject.GetComponentsInChildren<IActorDestroyHandler>())
            {
                handler.HandleDestroy(ActorDestroyReason.Health);
            }
            Destroy(gameObject);
        }
    }
}
