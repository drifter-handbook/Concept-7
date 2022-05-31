using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class OrbitTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "orbit";
    
    public float? Speed;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<OrbitTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        if (actor.transform.parent == null)
        {
            Debug.Log($"Warning: Cannot run 'orbit' on actor {actor.ActorType} because it has no parent.");
            return;
        }
        if (Speed != null)
        {
            actor.OrbitSpeed = Speed.Value;
        }
        actor.RunCoroutine(ref actor.movementCoroutine, actor.OrbitCoroutine());
    }
}