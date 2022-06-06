using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class SetSpeedTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "setspeed";

    public float Speed;
    public float Dur;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<SetSpeedTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        actor.RunCoroutine(ref actor.speedCoroutine, actor.SpeedCoroutine(Speed, Dur));
    }
}