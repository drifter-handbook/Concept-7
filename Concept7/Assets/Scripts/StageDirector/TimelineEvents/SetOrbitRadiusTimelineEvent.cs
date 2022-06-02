using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class SetOrbitRadiusTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "setorbitradius";

    public float Radius;
    public float Dur;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<SetOrbitRadiusTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        actor.RunCoroutine(ref actor.orbitRadiusCoroutine, actor.OrbitRadiusCoroutine(Radius, Dur));
    }
}