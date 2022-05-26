using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{

	public int health = 5;

    void OnTriggerExit2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "PlayArea")
    		Destroy(gameObject);
    	
    }

    void OnTriggerStay2D(Collider2D collider)
    {

    	if(collider.gameObject.tag == "PlayerWeapon")
    	{
    		health -= collider.gameObject.GetComponent<PlayerWeapon>().weaponData.damage;
    		UnityEngine.Debug.Log(health);
    	}
    		
    	if(health <=0)
    		Die();
    	
    }

    void Die()
    {
    	//play an animation here maybe?
    	Destroy(gameObject);
    }


}
