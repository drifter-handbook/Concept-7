using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorChainAttach : MonoBehaviour, IActorAttachment, IActorDestroyHandler
{
    int MAX_CHAIN = 10;

    public void Attach(StageActor target, StageActor source)
    {
        StartCoroutine(Chain());
    }

    public void HandleDestroy(ActorDestroyReason reason)
    {
        transform.parent = null;
    }

    IEnumerator Chain()
    {
        yield return null;
        // find closest other laser, and attach to it
        EngineTestConnectedLaser closest = null;
        int nearestIndex = -1;
        float minDist = float.MaxValue;
        foreach (EngineTestConnectedLaser las in FindObjectsOfType<EngineTestConnectedLaser>())
        {
            if (las.gameObject == gameObject)
            {
                continue;
            }
            float dist = ((Vector2)(las.transform.position - transform.position)).magnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = las;
            }
        }
        if (closest != null)
        {
            nearestIndex = closest.Index;
        }
        if (nearestIndex > MAX_CHAIN)
        {
            Destroy(gameObject);
            yield break;
        }
        EngineTestConnectedLaser laser = GetComponent<EngineTestConnectedLaser>();
        laser.Index = nearestIndex + 1;
        laser.Initialize();
    }
}
