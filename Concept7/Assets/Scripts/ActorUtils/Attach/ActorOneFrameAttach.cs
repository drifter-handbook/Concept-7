using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorOneFrameAttach : MonoBehaviour, IActorAttachment
{
    public List<StageActor.ActorClassification> Classifications;

    void Start()
    {
        StartCoroutine(DestroyInOneFrame());
    }

    public void Attach(StageActor target, StageActor source)
    {
        // destroy self if actor is wrong classification
        if (!Classifications.Contains(target.Classification))
        {
            Destroy(gameObject);
        }
    }

    IEnumerator DestroyInOneFrame()
    {
        yield return null;
        yield return null;
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
