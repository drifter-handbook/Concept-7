using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorDestroyOnImpact : MonoBehaviour, IActorCollisionHandler
{
    public void HandleCollision(GameObject other)
    {
        if (other.tag == "PlayArea")
        {
            return;
        }
        ActorSuppressOtherDestroyOnImpact suppress = other.GetComponent<ActorSuppressOtherDestroyOnImpact>();
        StageActor actor = GetComponent<StageActor>();
        if (suppress == null || actor == null || !suppress.Classifications.Contains(actor.Classification))
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        StageActor actor = GetComponent<StageActor>();
        if (gameObject != null && actor != null)
        {
            foreach (var handler in gameObject.GetComponentsInChildren<IActorDestroyHandler>())
            {
                handler.HandleDestroy(ActorDestroyReason.Impact);
            }
            Destroy(gameObject);
        }
    }
}
