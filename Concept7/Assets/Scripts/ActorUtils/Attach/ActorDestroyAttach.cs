using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorDestroyAttach : MonoBehaviour, IActorAttachment
{
    public List<StageActor.ActorClassification> Classifications;

    public void Attach(StageActor target, StageActor source)
    {
        // destroy target if actor is correct classification
        if (Classifications.Contains(target.Classification))
        {
            foreach (var handler in target.gameObject.GetComponentsInChildren<IActorDestroyHandler>())
            {
                handler.HandleDestroy(ActorDestroyReason.Impact);
            }
            Destroy(target.gameObject);
        }
        // cleanup self
        Destroy(gameObject);
    }
}
