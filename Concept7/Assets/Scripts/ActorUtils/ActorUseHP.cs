using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorUseHP : MonoBehaviour
{
	public float health;
    [SerializeField] private GameObject deathPrefab;

    public void Initialize(StageData.Actor actor)
    {
        health = actor.Hp ?? 1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ActorSuppressOtherUseHP suppress = other.GetComponent<ActorSuppressOtherUseHP>();
        StageActor actor = GetComponent<StageActor>();
        if (suppress == null || actor == null || !suppress.Classifications.Contains(actor.Classification))
        {
            if (other.gameObject.tag == "PlayerWeapon")
            {
                health -= (float?)other.gameObject.GetComponent<PlayerWeapon>()?.weaponData.damage ?? 0;
            }
            if (health <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        if (deathPrefab != null)
            Instantiate(deathPrefab, transform.position, Quaternion.identity);
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
