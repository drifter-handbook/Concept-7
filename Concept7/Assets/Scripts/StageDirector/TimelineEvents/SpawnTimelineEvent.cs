using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class SpawnTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
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
    public bool? MirrorX;
    public bool? MirrorY;
    public float? Lifetime;
    public string XModifier;
    public string YModifier;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<SpawnTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        Vector3 pos = FindDestPosition((X ?? 0) + GetVar(actor, XModifier), (Y ?? 0) + GetVar(actor, YModifier), Dir, Dist, Rel ?? "abs", actor.transform.position, actor.Direction);
        GameObject go = StageDirector.Spawn(Actor, new Vector3(pos.x, pos.y), 0f);
        StageActor spawned = go.GetComponent<StageActor>();
        float mirrorX = MirrorX == null ? actor.Mirror.x : (MirrorX.Value ? -1 : 1);
        float mirrorY = MirrorY == null ? actor.Mirror.y : (MirrorY.Value ? -1 : 1);
        spawned.Mirror = new Vector2(mirrorX, mirrorY);
        if (Parent != null)
        {
            go.transform.parent = StageDataUtils.GetParent(Parent, go.transform.position, actor.gameObject).transform;
        }
        spawned.FinishSpawn(Run, Lifetime);
        foreach (var handler in actor.gameObject.GetComponentsInChildren<IActorSpawnHandler>())
        {
            handler.HandleSpawn(spawned);
        }
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
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
        foreach (string v in new List<string> { XModifier, YModifier })
        {
            if (v != null && !current.Vars.ContainsKey(v))
            {
                throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} tries to use undefined variable {v}");
            }
        }
    }
}