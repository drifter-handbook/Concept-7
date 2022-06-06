using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class SetVarTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
{
    public string Action => "setvar";

    public string Var;
    public float? Set;
    public float? Inc;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<SetVarTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        if (Set != null)
        {
            actor.Vars[Var] = Set.Value;
        }
        if (Inc != null)
        {
            actor.Vars[Var] += Inc.Value;
        }
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        if (Set == null && Inc == null)
        {
            throw new StageDataException($"setvar command must have either 'set' or 'inc' field");
        }
        if (Var == null)
        {
            throw new StageDataException($"setvar command must have 'var' field");
        }
        if (!current.Vars.ContainsKey(Var))
        {
            throw new StageDataException($"Timeline setvar action in actor {current.Name} in file {current.File} tries to use undefined variable {Var}");
        }
    }
}