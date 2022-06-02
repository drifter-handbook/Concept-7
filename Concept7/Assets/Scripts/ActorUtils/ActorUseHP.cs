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

    void OnTriggerStay2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "PlayerWeapon")
    	{
    		health -= collider.gameObject.GetComponent<PlayerWeapon>().weaponData.damage;
    	}
    	if(health <= 0)
        {
            Die();
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
            actor.RunTimeline(actor.Actor.OnDestroy?.Impact);
            Destroy(gameObject);
        }
    }
}
