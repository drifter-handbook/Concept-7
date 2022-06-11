using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class DetachTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "detach";
    public bool Active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return new DetachTimelineEvent()
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
                if (actor.gameObject.GetComponent<ActorReattachTracker>())
                {
                    Debug.Log($"Warning: Actor {actor.ActorType} is already detached.");
                }
                else
                {
                    actor.gameObject.AddComponent<ActorReattachTracker>().Detach();
                }
            }
        }
    }
}