using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class MoveAtPlayerTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move_at_player";

    public float? MaxTurn;
    public float? Speed;
    public bool? Instant;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveAtPlayerTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        float speed = Speed ?? StageDirector.Instance.Data.Actors[actor.ActorType].Speed ?? 1;
        Vector2 target = PlayerController.Instance.transform.position;
        Vector2 dir = (target - (Vector2)runner.transform.position).normalized;
        Vector2 vel = dir.normalized * speed;
        if (MaxTurn != null)
        {
            if (actor.Velocity != Vector2.zero)
            {
                dir = actor.Velocity.normalized;
                float turn = Mathf.Clamp(Vector2.SignedAngle(dir, vel), -MaxTurn.Value, MaxTurn.Value);
                vel = Quaternion.Euler(0f, 0f, turn) * dir * speed;
            }
        }
        actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? (int?)0 : null));
    }
}
