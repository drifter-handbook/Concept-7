using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public WeaponData weaponData;

    void OnTriggerExit2D(Collider2D collider)
    {
    	if(collider.gameObject.tag == "PlayArea")
    		Destroy(gameObject);
    	
    }

}
