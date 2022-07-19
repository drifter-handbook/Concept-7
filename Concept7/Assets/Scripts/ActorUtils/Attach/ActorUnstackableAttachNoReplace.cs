using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorUnstackableAttachNoReplace : MonoBehaviour, IActorAttachment
{
    public void Attach(StageActor target, StageActor source)
    {
        StageActor actor = GetComponent<StageActor>();
        string actorType = GetComponent<StageActor>().ActorType;
        transform.parent = target.transform;
        // prevent attachment if there are attached actors of same type
        bool destroySelf = false;
        foreach (Transform child in target.transform)
        {
            StageActor ac = child.GetComponent<StageActor>();
            if (ac != null && ac?.ActorType == actorType && ac != actor)
            {
                destroySelf = true;
                break;
            }
        }
        if (destroySelf)
        {
            Destroy(gameObject);
        }
    }
}
