using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class StageData
{
    public List<Stage> Stages = new List<Stage>();
    public Dictionary<string, Actor> Actors = new Dictionary<string, Actor>();

    string WorkingDir => Application.streamingAssetsPath;
    const string StagesFile = "stages.yaml";

    const string EmitterPrefix = "emitter_";
    const string TimelinePrefix = "timeline_";

    List<Actor.Timeline.IEvent> TimelineEvents = new List<Actor.Timeline.IEvent>()
    {

    };

    public StageData()
    {
        Load();
    }

    public void Load()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        string stagesPath = Path.Combine(WorkingDir, StagesFile);
        Stages = ToStages(stagesPath, deserializer);
        foreach (string p in Directory.GetFiles(WorkingDir, "*.yaml", SearchOption.AllDirectories))
        {
            if (p == stagesPath)
            {
                continue;
            }
            Actor actor = ToActor(p, deserializer, serializer);
            if (Actors.ContainsKey(actor.Name))
            {
                throw new InvalidOperationException($"Duplicate actor {actor.Name} in both {actor.File} and {Actors[actor.Name].File}");
            }
            Actors[actor.Name] = actor;
        }
        CheckRefs();
    }

    private List<Stage> ToStages(string path, IDeserializer deserializer)
    {
        StageList stages = deserializer.Deserialize<StageList>(File.ReadAllText(path));
        return stages.Stages;
    }
    private Actor ToActor(string path, IDeserializer deserializer, ISerializer serializer)
    {
        Dictionary<string, object> actorData = deserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));
        Actor actor = deserializer.Deserialize<Actor>(serializer.Serialize(actorData["core"]));
        actor.File = path;
        if (string.IsNullOrEmpty(actor.Name))
        {
            throw new InvalidOperationException($"Failed to load {actor.File}: Missing actor name");
        }
        foreach (string s in actorData.Keys)
        {
            if (s.StartsWith("emitter_"))
            {
                string name = s.Remove(s.IndexOf(EmitterPrefix), EmitterPrefix.Length);
                Actor.Emitter em = deserializer.Deserialize<Actor.Emitter>(serializer.Serialize(actorData[s]));
                em.Name = name;
                if (actor.Emitters.ContainsKey(em.Name))
                {
                    throw new InvalidOperationException($"Duplicate emitter {em.Name} in {actor.File}");
                }
                actor.Emitters[em.Name] = em;
            }
            if (s.StartsWith("timeline_"))
            {
                string name = s.Remove(s.IndexOf(TimelinePrefix), TimelinePrefix.Length);
                Actor.Timeline timeline = new Actor.Timeline() { Name = name };
                if (actor.Timelines.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Duplicate timeline {name} in {actor.File}");
                }
                actor.Timelines[name] = timeline;
                foreach (Dictionary<object, object> evdata in (List<object>)actorData[s])
                {
                    Dictionary<string, object> evconv = evdata.ToDictionary(x => (string)x.Key, x => x.Value);
                    Dictionary<string, object> ev = new Dictionary<string, object>(evconv);
                    Dictionary<string, object> ev2 = new Dictionary<string, object>();
                    foreach (string field in ev.Keys)
                    {
                        if (!Actor.Timeline.Entry.Fields.Contains(field))
                        {
                            ev2[field] = ev[field];
                        }
                        if (ev2.Count > 1)
                        {
                            List<string> actions = ev2.Keys.ToList();
                            throw new InvalidOperationException($"Timeline entry in timeline {name}, file {actor.File} contains more than one action: {actions[0]} and {actions[1]}");
                        }
                    }
                    if (ev2.Count == 0)
                    {
                        throw new InvalidOperationException($"Timeline entry in timeline {name}, file {actor.File} contains no action.");
                    }
                    ev = ev.Where(x => Actor.Timeline.Entry.Fields.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    Actor.Timeline.Entry ste = deserializer.Deserialize<Actor.Timeline.Entry>(serializer.Serialize(ev));
                    string action = ev2.Keys.First();
                    foreach (Actor.Timeline.IEvent evt in TimelineEvents)
                    {
                        if (evt.Action == action)
                        {
                            ste.Event = evt.CloneFrom(serializer.Serialize(ev2[action]), deserializer);
                        }
                    }
                    timeline.Entries.Add(ste);
                }
            }
        }
        return actor;
    }
    // check references to actors, and handle copyFrom's
    private void CheckRefs()
    {

    }

    class StageList
    {
        public List<Stage> Stages;
    }
    public class Stage
    {
        public string Name;
        public string Actor;
    }
    public class Actor
    {
        public string File;
        public string Name;
        public string CopyFrom;
        public string Prefab;
        public bool? Invuln;
        public int? Hp;
        public string DefaultRun;
        public bool? TurnOnMove;
        public List<string> Tags;
        public float? Speed;
        public float? Depth;
        public Dictionary<string, Emitter> Emitters = new Dictionary<string, Emitter>();
        public Dictionary<string, Timeline> Timelines = new Dictionary<string, Timeline>();

        public class Emitter
        {
            public string Name;
            public float x;
            public float y;
            public string Actor;
        }
        public class Timeline
        {
            public string Name;
            public List<Entry> Entries = new List<Entry>();

            public class Entry
            {
                public static IList<string> Fields = new List<string> { "time", "repeat", "interval" }.AsReadOnly();
                public float Time;
                public int Repeat;
                public float Interval;
                public IEvent Event;
            }

            public interface IEvent
            {
                string Action { get; }
                IEvent CloneFrom(string yaml, IDeserializer deserializer);
                void Start(MonoBehaviour runner);
                void Stop(MonoBehaviour runner);
            }
        }
    }
}

public class SpawnTimelineEvent : StageData.Actor.Timeline.IEvent
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
        // TODO: spawn actor
    }

    public void Stop(MonoBehaviour runner) {}
}

public class ShootAtPlayerTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "shoot_at_player";

    int Num;
    float Angle;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<ShootAtPlayerTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        // TODO: spawn actor, set actor speed and direction
    }

    public void Stop(MonoBehaviour runner) { }
}

public class MoveTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move";

    public float X;
    public float Y;
    public float Dir;
    public float Dist;
    public float Speed;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        // TODO: kick off move coroutine
    }

    public void Stop(MonoBehaviour runner)
    {
        // TODO: stop coroutine
    }
}

public class MoveAbsTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move_abs";

    public float X;
    public float Y;
    public float Speed;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveAbsTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        // TODO: kick off move coroutine
    }

    public void Stop(MonoBehaviour runner)
    {
        // TODO: stop coroutine
    }
}

public class DestroyTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroy";
    bool active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return new DestroyTimelineEvent()
        {
            active = deserializer.Deserialize<bool>(yaml)
        };
    }

    public void Start(MonoBehaviour runner)
    {
        if (active)
        {
            UnityEngine.Object.Destroy(runner.gameObject);
        }
    }

    public void Stop(MonoBehaviour runner) { }
}