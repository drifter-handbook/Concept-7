using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class SpawnTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.IActorCheck
{
    public string Action => "spawn";
    public static List<string> ParentValues = new List<string>(){ null, "emitter", "actor", "new" };

    public string Actor;
    public float? X;
    public float? Y;
    public float? Dir;
    public float? Dist;
    public string Rel;
    public string Run;
    public string Parent;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<SpawnTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        Vector3 pos = TimelineEventUtils.FindDestPosition(X, Y, Dir, Dist, Rel ?? "abs", runner.transform.position, actor.Direction);
        GameObject go = StageDirector.Spawn(Actor, new Vector3(pos.x, pos.y), 0f);
        StageActor spawned = go.GetComponent<StageActor>();
        if (Parent != null)
        {
            go.transform.parent = TimelineEventUtils.GetParent(Parent, go.transform.position, runner.gameObject).transform;
        }
        spawned.RunTimeline(Run ?? StageDirector.Instance.Data.Actors[Actor].DefaultRun);
    }

    public void CheckActors(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        if (Actor == null)
        {
            throw new StageDataException($"Timeline spawn action in actor {current.Name} in file {current.File} is missing 'actor' field.");
        }
        if (!actors.ContainsKey(Actor))
        {
            throw new StageDataException($"Timeline spawn action in actor {current.Name} in file {current.File} attempts to spawn {Actor} which does not exist.");
        }
        if (Parent != null && (Parent == "emitter" || !ParentValues.Contains(Parent.ToLower())))
        {
            throw new StageDataException($"Timeline spawn action in actor {current.Name} in file {current.File} has 'parent' field {Parent} where the only allowed values are [null, 'new', 'actor']");
        }
    }
}