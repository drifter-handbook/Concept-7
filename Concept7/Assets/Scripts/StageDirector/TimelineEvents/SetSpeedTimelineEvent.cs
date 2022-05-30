using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class SetSpeedTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "setspeed";

    public float Speed;
    public float Dur;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<SetSpeedTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        actor.RunSpeedCoroutine(actor.SpeedCoroutine(Speed, Dur));
    }
}