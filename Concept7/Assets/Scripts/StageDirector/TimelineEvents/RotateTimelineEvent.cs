using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class RotateTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "rotate";
    
    public float? Set;
    public float? Inc;
    public float? Dur;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<RotateTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        float angle = actor.transform.position.z + Inc.Value;
        if (Set != null)
        {
            angle = actor.transform.position.z + Vector2.SignedAngle(Quaternion.Euler(0f, 0f, actor.transform.position.z) * Vector2.right, Quaternion.Euler(0f, 0f, Set ?? 0) * Vector2.right);
        }
        actor.RunCoroutine(ref actor.rotateCoroutine, actor.RotateCoroutine(angle, Dur ?? 0));
    }
}