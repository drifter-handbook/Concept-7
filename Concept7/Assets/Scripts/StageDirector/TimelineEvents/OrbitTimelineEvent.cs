using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class OrbitTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "orbit";
    
    public float? Speed;
    public float? Radius;
    public float? Dur;
    public Vector3? Tilt;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<OrbitTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        if (actor.transform.parent == null)
        {
            Debug.Log($"Warning: Cannot run 'orbit' on actor {actor.ActorType} because it has no parent.");
            return;
        }
        if (Speed != null)
        {
            actor.RunCoroutine(ref actor.orbitSpeedCoroutine, actor.OrbitSpeedCoroutine(Speed.Value, Dur ?? 0));
        }
        if (Radius != null)
        {
            actor.RunCoroutine(ref actor.orbitRadiusCoroutine, actor.OrbitRadiusCoroutine(Radius.Value, Dur ?? 0));
        }
        if (Tilt != null)
        {
            actor.RunCoroutine(ref actor.orbitTiltCoroutine, actor.OrbitTiltCoroutine(Tilt.Value, Dur ?? 1));
        }
        actor.RunCoroutine(ref actor.movementCoroutine, actor.OrbitCoroutine());
    }
}