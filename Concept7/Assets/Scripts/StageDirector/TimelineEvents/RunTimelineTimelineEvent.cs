using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class RunTimelineTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
{
    public string Action => "run";

    public string Timeline;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<RunTimelineTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        actor.RunTimeline(Timeline);
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        if (Timeline != null && !current.Timelines.ContainsKey(Timeline))
        {
            throw new StageDataException($"Timeline run action in actor {current.Name} in file {current.File} attempts to run timeline_{Timeline} which does not exist.");
        }
    }
}