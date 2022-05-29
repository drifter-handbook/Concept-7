using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{

	[SerializeField] private int health = 5;
	[SerializeField] private Animator healthBar;

	public void OnTriggerEnter2D(Collider2D other) {
        health--;
        //Play hurt animation?
        if(other.gameObject.tag == "Enemy")
        {
        	healthBar.SetInteger("Health",health);
            StartCoroutine(flickerHurtbox());
            UnityEngine.Debug.Log("OUCH");
            if(health <=0)
                UnityEngine.Debug.Log("DED");
        }
        
    }

    private IEnumerator flickerHurtbox()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(1f);
        GetComponent<CircleCollider2D>().enabled = true;
    }
}
