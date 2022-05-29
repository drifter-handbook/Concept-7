using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class MoveTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move";

    public float? X;
    public float? Y;
    public float? Dir;
    public float? Dist;
    public float? Speed;
    public bool? Instant;
    public float Smoothness;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        float speed = Speed ?? StageDirector.Instance.Data.Actors[actor.ActorType].Speed ?? 1;
        if (X != null || Y != null)
        {
            Vector2 diff = new Vector2(X ?? 0, Y ?? 0);
            Vector2 vel = diff.normalized * speed;
            float dur = diff.magnitude / speed;
            Vector2 target = (Vector2)runner.transform.position + diff;
            actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target, Smoothness));
        }
        else if (Dir != null)
        {
            Vector2 vel = Quaternion.Euler(0, 0, Dir.Value) * Vector2.right * speed;
            if (Dist != null)
            {
                float dur = Dist.Value / speed;
                Vector2 target = (Vector2)runner.transform.position + dur * vel;
                actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target, Smoothness));
            }
            else
            {
                actor.RunMoveCoroutine(actor.MoveCoroutine(vel));
            }
        }
        else if (Dist != null)
        {
            Vector2 vel = actor.Direction * speed;
            if (Dist != null)
            {
                float dur = Dist.Value / speed;
                Vector2 target = (Vector2)runner.transform.position + dur * vel;
                actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target, Smoothness));
            }
            else
            {
                actor.RunMoveCoroutine(actor.MoveCoroutine(vel));
            }
        }
        else
        {
            // stop if no args
            actor.RunMoveCoroutine(actor.MoveCoroutine(Vector2.zero));
        }
    }
}