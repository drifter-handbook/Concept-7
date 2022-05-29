using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class DestroyTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroy";
    public bool active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return new DestroyTimelineEvent()
        {
            active = deserializer.Deserialize<bool>(yaml)
        };
    }

    public void Start(MonoBehaviour runner)
    {
        if (active)
        {
            UnityEngine.Object.Destroy(runner.gameObject);
        }
    }
}