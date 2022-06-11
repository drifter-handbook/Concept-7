using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class ReattachTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "reattach";
    public bool Active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return new ReattachTimelineEvent()
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
                var reattach = actor.gameObject.GetComponent<ActorReattachTracker>();
                if (reattach == null)
                {
                    Debug.Log($"Warning: Actor {actor.ActorType} is not detached.");
                }
                else
                {
                    reattach.Reattach();
                }
            }
        }
    }
}