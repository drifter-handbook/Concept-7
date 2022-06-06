using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class DestroyTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroy";
    public bool Active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return new DestroyTimelineEvent()
        {
            Active = Deserialize<bool>(actor, $"Timeline event {Action}", yaml)
        };
    }

    public void Start(StageActor actor)
    {
        if (Active)
        {
            if (actor != null)
            {
                foreach (var handler in actor.gameObject.GetComponentsInChildren<IActorDestroyHandler>())
                {
                    handler.HandleDestroy(ActorDestroyReason.Event);
                }
                UnityEngine.Object.Destroy(actor.gameObject);
            }
        }
    }
}