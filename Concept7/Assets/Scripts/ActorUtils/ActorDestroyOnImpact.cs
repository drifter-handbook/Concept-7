using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyOnImpact : MonoBehaviour
{
    [SerializeField] private GameObject impactPrefab;
    private void OnTriggerEnter2D(Collider2D other)
    {
    	if(other.gameObject.tag != "PlayArea")
        {
            StartCoroutine(DestroyAfterDelay());
            if (impactPrefab != null)
                Instantiate(impactPrefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator DestroyAfterDelay() {
        yield return new WaitForSeconds(Time.fixedDeltaTime);
        Destroy(gameObject);
    }
}
