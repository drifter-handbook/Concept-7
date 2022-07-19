using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorUnstackableAttach : MonoBehaviour, IActorAttachment
{
    public void Attach(StageActor target, StageActor source)
    {
        StageActor actor = GetComponent<StageActor>();
        string actorType = GetComponent<StageActor>().ActorType;
        transform.parent = target.transform;
        // remove other attached actors of same type
        foreach (Transform child in target.transform)
        {
            StageActor ac = child.GetComponent<StageActor>();
            if (ac != null && ac?.ActorType == actorType && ac != actor)
            {
                child.gameObject.transform.parent = null;
                Destroy(child.gameObject);
            }
        }
    }
}
