using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;
using System.Linq;

public class MoveAtTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
{
    public string Action => "move_at";

    public float? MaxTurn;
    public float? Speed;
    public bool? Instant;

    public string GameTag;
    public string Actor;
    public List<string> Classification;
    private List<StageActor.ActorClassification> ClassificationEnums = new List<StageActor.ActorClassification>();

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<MoveAtTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        List<StageActor> actorList = GameObject.FindGameObjectsWithTag(GameTag).Select(x => x.GetComponent<StageActor>()).Where(x => x != null).ToList();
        if (Actor != null)
        {
            actorList = actorList.Where(x => x.ActorType == Actor).ToList();
        }
        if (Classification != null)
        {
            actorList = actorList.Where(x => ClassificationEnums.Contains(x.Classification)).ToList();
        }
        StageActor targetActor = NearestActor(actorList, actor);
        if (targetActor == null)
        {
            // Debug.Log($"No GameObjects found with Tag '{GameTag}'{(Actor != null ? ", Actor=" + Actor : "")}{(Classification != null && Classification.Count > 0 ? ", Classification=[" + string.Join(", ", Classification) + "]" : "")}");
            return;
        }
        // stop currently running move coroutine
        actor.RunCoroutine(ref actor.movementCoroutine, null);
        Vector2 target = targetActor.transform.position;
        Vector2 dir = (target - (Vector2)actor.transform.position).normalized;
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
        if (Instant ?? false)
        {
            actor.transform.position = target;
        }
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        if (GameTag == null)
        {
            throw new StageDataException($"Timeline move_at action in actor {current.Name} in file {current.File} is missing 'game_tag' field.");
        }
        if (Classification != null)
        {
            foreach (string s in Classification)
            {
                try
                {
                    ClassificationEnums.Add((StageActor.ActorClassification)Enum.Parse(typeof(StageActor.ActorClassification), s));
                }
                catch (ArgumentException)
                {
                    throw new StageDataException($"Timeline move_at action in actor {current.Name} in file {current.File} uses invalid value '{s}' in 'classification' field. The only allowed values are {string.Join(", ", Enum.GetNames(typeof(StageActor.ActorClassification)))}");
                }
            }
        }
    }
}
