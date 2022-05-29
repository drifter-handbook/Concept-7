using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class SpawnTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.IActorCheck
{
    public string Action => "spawn";

    public string Actor;
    public float X;
    public float Y;
    public string Run;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<SpawnTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageDirector.Spawn(Actor, new Vector3(X, Y), 0f, Run);
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
    }
}