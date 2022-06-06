using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyOnImpact : MonoBehaviour
{
    [SerializeField] private GameObject impactPrefab;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "PlayArea")
        {
            return;
        }
        ActorSuppressOtherDestroyOnImpact suppress = other.GetComponent<ActorSuppressOtherDestroyOnImpact>();
        StageActor actor = GetComponent<StageActor>();
        if (suppress == null || actor == null || !suppress.Classifications.Contains(actor.Classification))
        {
            StartCoroutine(DestroyAfterDelay());
            if (impactPrefab != null)
                Instantiate(impactPrefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator DestroyAfterDelay() {
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
