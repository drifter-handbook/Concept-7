using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class DestroyOtherTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroyother";
    public string Actor;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<DestroyOtherTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        foreach (StageActor sa in StageDirector.Instance.CurrentActors)
        {
            if (sa.ActorType != Actor)
            {
                continue;
            }
            foreach (var handler in sa.gameObject.GetComponentsInChildren<IActorDestroyHandler>())
            {
                handler.HandleDestroy(ActorDestroyReason.Event);
            }
            sa.transform.parent = null;
            UnityEngine.Object.Destroy(sa.gameObject);
        }
    }
}