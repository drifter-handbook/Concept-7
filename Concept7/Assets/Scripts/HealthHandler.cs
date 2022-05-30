using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    //lives are now tracked in the Game script, but impact is still handled here
    Game game;

    public void Start(){
        game = FindObjectOfType<Game>();
    }

	void OnTriggerEnter2D(Collider2D col)
    {
        game.LivesChanged(-1);
        //Play hurt animation?
        if(col.gameObject.tag == "Enemy")
        {
        	//healthBar.SetInteger("Health",health);
            StartCoroutine(flickerHurtbox());
        }
    }

    private IEnumerator flickerHurtbox()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(1f);
        GetComponent<CircleCollider2D>().enabled = true;
    }
}
