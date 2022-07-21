using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class CallTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "call";

    public string Event;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<CallTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        ActorUnityEvents ev = actor.GetComponent<ActorUnityEvents>();
        if (ev == null || !ev.Call(Event))
        {
            Debug.Log($"Warning: Event {Event} not registered with ActorUnityEvents component on actor {actor.ActorType}");
        }
    }
}