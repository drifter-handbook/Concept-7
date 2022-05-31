using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class SetOrbitSpeedTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "setorbitspeed";

    public float Speed;
    public float Dur;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<SetOrbitSpeedTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        actor.RunCoroutine(ref actor.orbitSpeedCoroutine, actor.OrbitSpeedCoroutine(Speed, Dur));
    }
}