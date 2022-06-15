using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorOriginalPositionAttach : MonoBehaviour, IActorAttachment
{
    public void Attach(StageActor target, StageActor source)
    {
        transform.position = source.transform.position;
    }
}
