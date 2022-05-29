using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyOnImpact : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
    	if(other.gameObject.tag != "PlayArea")
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay() {
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        Destroy(gameObject);
    }
}
