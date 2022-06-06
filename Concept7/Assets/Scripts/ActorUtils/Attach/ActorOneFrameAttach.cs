using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorOneFrameAttach : MonoBehaviour, IActorAttachment
{
    public void Attach(StageActor target, StageActor source)
    {
    }

    IEnumerator DestroyInOneFrame()
    {
        yield return null;
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
