using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class MoveAtPlayerTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move_at_player";

    public float? MaxTurn;
    public float? Speed;
    public bool? Instant;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<MoveAtPlayerTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        // stop currently running move coroutine
        actor.RunCoroutine(ref actor.movementCoroutine, null);
        Vector2 target = PlayerController.Instance.transform.position;
        // TODO: this is not performant at all
        if (runner.tag == "PlayerWeapon")
        {
            target = (Vector2)actor.transform.position + actor.Direction * 100f;
            foreach (GameObject go in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                if (go.GetComponent<ActorUseHP>() != null && !go.name.ToLower().Contains("missile"))
                {
                    if (((Vector2)(go.transform.position - actor.transform.position)).magnitude < ((Vector2)go.transform.position - target).magnitude)
                    {
                        target = go.transform.position;
                    }
                }
            }
        }
        Vector2 dir = (target - (Vector2)runner.transform.position).normalized;
        if (MaxTurn != null)
        {
            float turn = Mathf.Clamp(Vector2.SignedAngle(actor.Direction, dir), -MaxTurn.Value, MaxTurn.Value);
            dir = Quaternion.Euler(0f, 0f, turn) * actor.Direction;
        }
        actor.Direction = dir;
        if (Speed != null)
        {
            actor.Speed = Speed.Value;
        }
    }
}
