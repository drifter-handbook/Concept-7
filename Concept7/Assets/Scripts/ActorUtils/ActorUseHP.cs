using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorUseHP : MonoBehaviour
{
	public float health;

    public void Initialize(StageData.Actor actor)
    {
        health = actor.Hp ?? 1;
    }

    void OnTriggerStay2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "PlayerWeapon")
    	{
    		health -= collider.gameObject.GetComponent<PlayerWeapon>().weaponData.damage;
    		Debug.Log(health);
    	}
    	if(health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
    	//play an animation here maybe?
    	Destroy(gameObject);
    }
}
