using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class HealthHandler : MonoBehaviour, IActorCollisionHandler
{
    //lives are now tracked in the Game script, but impact is still handled here
    Game game;

    public int Order => 1;

    public void Start(){
        game = FindObjectOfType<Game>();
    }

    private IEnumerator flickerHurtbox()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        yield return new WaitForSeconds(1f);
        GetComponent<CircleCollider2D>().enabled = true;
    }

    public void HandleCollision(GameObject other)
    {
        StageActor actor = GetComponent<StageActor>();
        if (actor != null)
        {
            ActorSuppressOtherUseHP suppress = other.GetComponent<ActorSuppressOtherUseHP>();
            if (suppress == null || !suppress.Classifications.Contains(actor.Classification))
            {
                game.LivesChanged(-1);
                //Play hurt animation?
                if (gameObject.tag == "Enemy")
                {
                    //healthBar.SetInteger("Health",health);
                    StartCoroutine(flickerHurtbox());
                }
            }
        }
    }
}
