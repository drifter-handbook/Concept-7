using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class ScaleTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "scale";
    
    public float? X;
    public float? Y;
    public float? Z;
    public float? Dur;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<ScaleTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        actor.RunCoroutine(ref actor.scaleCoroutine, actor.ScaleCoroutine(new Vector3(X ?? actor.transform.localScale.x, Y ?? actor.transform.localScale.y, Z ?? actor.transform.localScale.z), Dur ?? 0));
    }
}