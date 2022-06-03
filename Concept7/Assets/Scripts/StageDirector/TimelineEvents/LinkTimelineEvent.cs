using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class LinkTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
{
    public string Action => "link";

    public string Actor;
    public string FromActor;
    public string FromTimeline;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<LinkTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        GameObject link = StageDirector.FindCurrentActor(Actor);
        // if link actor doesn't exist, create it
        if (link == null)
        {
            // find average positions of specified actors running the specified timeline
            Vector2 sum = Vector2.zero;
            int count = 0;
            foreach (StageActor x in StageDirector.Instance.CurrentActors)
            {
                if (x != null && x.Actor.Name == FromActor && (FromTimeline == null || x.IsRunningTimeline(FromTimeline)))
                {
                    sum += (Vector2)x.transform.position;
                    count++;
                }
            }
            count = Math.Max(count, 1);
            link = StageDirector.Spawn(Actor, sum / count, 0f);
            link.GetComponent<StageActor>().FinishSpawn();
        }
        // parent to link actors to
        actor.transform.parent = link.transform;
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        if (FromActor == null)
        {
            throw new StageDataException($"Timeline link action in actor {current.Name} in file {current.File} has no 'from_actor' field.");
        }
        if (!actors.ContainsKey(Actor))
        {
            throw new StageDataException($"Timeline link action in actor {current.Name} in file {current.File} tries to use undefined actor {Actor}");
        }
        if (!actors.ContainsKey(FromActor))
        {
            throw new StageDataException($"Timeline link action in actor {current.Name} in file {current.File} tries to use undefined actor {FromActor}");
        }
        if (FromTimeline != null && !actors[FromActor].Timelines.ContainsKey(FromTimeline))
        {
            throw new StageDataException($"Timeline link action in actor {current.Name} in file {current.File} tries to use undefined timeline {FromTimeline} in actor {FromActor}");
        }
    }
}