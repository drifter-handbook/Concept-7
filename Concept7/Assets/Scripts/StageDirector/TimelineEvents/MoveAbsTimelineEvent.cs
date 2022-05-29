using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class MoveAbsTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move_abs";

    public float X;
    public float Y;
    public float? Speed;
    public bool? Instant;
    public float Smoothness;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveAbsTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        float speed = Speed ?? StageDirector.Instance.Data.Actors[actor.ActorType].Speed ?? 1;
        Vector2 target = new Vector2(X, Y);
        Vector2 diff = target - (Vector2)runner.transform.position;
        Vector2 vel = diff.normalized * speed;
        float dur = diff.magnitude / speed;
        actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target, Smoothness));
    }
}