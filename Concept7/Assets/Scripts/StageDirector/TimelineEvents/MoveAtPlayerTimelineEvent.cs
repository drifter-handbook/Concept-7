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
        Vector2 target = PlayerController.Instance.transform.position;
        Vector2 dir = (target - (Vector2)runner.transform.position).normalized;
        if (MaxTurn != null)
        {
            float turn = Mathf.Clamp(Vector2.SignedAngle(actor.Direction, dir), -MaxTurn.Value, MaxTurn.Value);
            dir = Quaternion.Euler(0f, 0f, turn) * dir;
        }
        actor.Direction = dir;
        if (Speed != null)
        {
            actor.Speed = Speed.Value;
        }
    }
}
