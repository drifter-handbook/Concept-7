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
    // Info on "If Stage N starts, what actor should I create to run the stage?"
    public List<Stage> Stages = new List<Stage>();

    // Holds all actor types, and their data, emitters, and timelines
    public Dictionary<string, Actor> Actors = new Dictionary<string, Actor>();

    string WorkingDir => Application.streamingAssetsPath;
    const string StagesFile = "stages.yaml";

    const string EmitterPrefix = "emitter_";
    const string TimelinePrefix = "timeline_";

    List<Actor.Timeline.IEvent> TimelineEvents = new List<Actor.Timeline.IEvent>()
    {
        new SpawnTimelineEvent(), new ShootAtPlayerTimelineEvent(), new MoveTimelineEvent(), new MoveAbsTimelineEvent(), new MoveAtPlayerTimelineEvent(), new DestroyTimelineEvent()
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
        // parse stages file
        string stagesPath = Path.Combine(WorkingDir, StagesFile);
        Stages = ToStages(stagesPath, deserializer);
        // parse all actor files
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
        // load actor core fields
        Dictionary<string, object> actorData = deserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));
        Actor actor = deserializer.Deserialize<Actor>(serializer.Serialize(actorData["core"]));
        actor.File = path;
        if (string.IsNullOrEmpty(actor.Name))
        {
            throw new InvalidOperationException($"Failed to load {actor.File}: Missing actor name");
        }
        // load emitters and timelines
        foreach (string s in actorData.Keys)
        {
            // load emitters
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
            // load timelines
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
                    // every event in the timeline must have exactly one "action" field, such as "move" or "shoot_at_player"
                    Dictionary<string, object> evconv = evdata.ToDictionary(x => (string)x.Key, x => x.Value);
                    Dictionary<string, object> ev = new Dictionary<string, object>(evconv);
                    if (!ev.ContainsKey("time"))
                    {
                        throw new InvalidOperationException($"Timeline entry in timeline {name}, file {actor.File} contains no 'time' field.");
                    }
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
                    // parse action field
                    ev = ev.Where(x => Actor.Timeline.Entry.Fields.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                    Actor.Timeline.Entry ste = deserializer.Deserialize<Actor.Timeline.Entry>(serializer.Serialize(ev));
                    string action = ev2.Keys.First();
                    foreach (Actor.Timeline.IEvent evt in TimelineEvents)
                    {
                        if (evt.Action == action)
                        {
                            if (ste.Event != null)
                            {
                                throw new InvalidOperationException($"Action {action} matches more than one action in StageData.TimelineEvents!");
                            }
                            ste.Event = evt.CloneFrom(serializer.Serialize(ev2[action]), deserializer);
                        }
                    }
                    if (ste.Event == null)
                    {
                        throw new InvalidOperationException($"Action {action} is not registered in StageData.TimelineEvents");
                    }
                    timeline.Entries.Add(ste);
                }
            }
        }
        return actor;
    }
    // check references to actors
    private void CheckRefs()
    {
        for (int i = 0; i < 2; i++)
        {
            foreach (var p in Actors)
            {
                p.Value.EnsureCopy(Actors);
            }
        }
        foreach (var p in Actors)
        {
            p.Value.Check(Actors);
        }
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
        public GameObject PrefabObj;
        public bool? Invuln;
        public int? Hp;
        public string DefaultRun;
        public bool? TurnOnMove;
        public List<string> Tags;
        public float? Speed;
        public float? Depth;
        public Dictionary<string, Emitter> Emitters = new Dictionary<string, Emitter>();
        public Dictionary<string, Timeline> Timelines = new Dictionary<string, Timeline>();

        // TODO: DAG copy_from check
        public void EnsureCopy(Dictionary<string, Actor> actors)
        {
            // handle copy_from
            if (CopyFrom != null)
            {
                if (!actors.ContainsKey(CopyFrom))
                {
                    throw new InvalidOperationException($"Actor {Name} in file {File} attempts to copy_from {CopyFrom} which does not exist.");
                }
                Actor copySrc = actors[CopyFrom];
                // copy over core fields
                // The current object's fields take priority
                if (Prefab == null) { Prefab = copySrc.Prefab; }
                if (Invuln == null) { Invuln = copySrc.Invuln; }
                if (Hp == null) { Hp = copySrc.Hp; }
                if (DefaultRun == null) { DefaultRun = copySrc.DefaultRun; }
                if (TurnOnMove == null) { TurnOnMove = copySrc.TurnOnMove; }
                if (Tags == null) { Tags = copySrc.Tags; }
                if (Speed == null) { Speed = copySrc.Speed; }
                if (Depth == null) { Depth = copySrc.Depth; }
                // copy over emitters and timelines
                // The current object's take priority again
                foreach (string key in copySrc.Emitters.Keys)
                {
                    if (!Emitters.ContainsKey(key))
                    {
                        Emitters[key] = copySrc.Emitters[key];
                    }
                }
                foreach (string key in copySrc.Timelines.Keys)
                {
                    if (!Timelines.ContainsKey(key))
                    {
                        Timelines[key] = copySrc.Timelines[key];
                    }
                }
            }
        }
        public void Check(Dictionary<string, Actor> actors)
        {
            // check prefab exists
            if (Prefab != null)
            {
                var prefabObjs = StageDirector.Instance.Prefabs.Where(x => x.name == Prefab).ToList();
                if (prefabObjs.Count == 0)
                {
                    throw new InvalidOperationException($"Actor {Name} in file {File} attempts to use prefab {Prefab} which is not registered with StageDirector.Prefabs");
                }
                if (prefabObjs.Count > 1)
                {
                    throw new InvalidOperationException($"Actor {Name} in file {File} attempts to use prefab {Prefab} which has too many ({prefabObjs.Count}) matches in StageDirector.Prefabs");
                }
                PrefabObj = prefabObjs.First();
            }
            else
            {
                PrefabObj = StageDirector.Instance.DefaultActorPrefab;
            }
            // check emitters and timelines for nonexistent actor types
            foreach (Emitter em in Emitters.Values)
            {
                em.CheckActors(actors, this);
            }
            foreach (Timeline t in Timelines.Values)
            {
                foreach (Timeline.Entry entry in t.Entries)
                {
                    IActorCheck check = entry.Event as IActorCheck;
                    if (check != null)
                    {
                        check.CheckActors(actors, this);
                    }
                }
                // make sure timelines are in sorted order
                t.Entries = t.Entries.OrderBy(x => x.Time).ToList();
            }
        }

        public Emitter DefaultEmitter => Emitters.Count > 0 ? Emitters.First().Value : null;

        public class Emitter : IActorCheck
        {
            public string Name;
            public float X;
            public float Y;
            public string Actor;

            public void CheckActors(Dictionary<string, Actor> actors, Actor current)
            {
                if (Actor != null && !actors.ContainsKey(Actor))
                {
                    throw new InvalidOperationException($"Emitter {Name} in actor {current.Name} in file {current.File} attempts to emit {Actor} which does not exist.");
                }
            }
        }
        public class Timeline
        {
            public string Name;
            public List<Entry> Entries = new List<Entry>();

            public class Entry
            {
                public static IList<string> Fields = new List<string> { "time", "repeat", "interval" }.AsReadOnly();
                public float Time;
                public int? Repeat;
                public float? Interval;
                public IEvent Event;
            }
            // Template for actions that occur within a timeline
            public interface IEvent
            {
                string Action { get; }
                IEvent CloneFrom(string yaml, IDeserializer deserializer);
                void Start(MonoBehaviour runner);
            }
        }
        // Checkable for actor references within actually existing
        public interface IActorCheck
        {
            // throw if an actor within doesn't exist
            void CheckActors(Dictionary<string, Actor> actors, Actor current);
        }
    }
}

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
            throw new InvalidOperationException($"Timeline spawn action in actor {current.Name} in file {current.File} is missing 'actor' field.");
        }
        if (!actors.ContainsKey(Actor))
        {
            throw new InvalidOperationException($"Timeline spawn action in actor {current.Name} in file {current.File} attempts to spawn {Actor} which does not exist.");
        }
    }
}

public class ShootAtPlayerTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.IActorCheck
{
    public string Action => "shoot_at_player";

    public int? Num;
    public float Angle;
    public string Emitter;
    public string Actor;
    public string Run;
    public float? Speed;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<ShootAtPlayerTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        // find emitter
        GameObject em = runner.GetComponent<StageActor>().Emitters[Emitter];
        float speed = Speed ?? StageDirector.Instance.Data.Actors[Actor].Speed ?? 1;
        Vector2 toPlayer = ((Vector2)PlayerController.Instance.transform.position - (Vector2)em.transform.position).normalized;
        // create Num shots with Angle spread
        int shots = (Num ?? 1);
        for (int i = 0; i < shots; i++)
        {
            float angle = Mathf.Lerp(Angle * -0.5f, Angle * 0.5f, (float)i / shots);
            StageDirector.Spawn(Actor, em.transform.position, 0f, Run);
            runner.GetComponent<StageActor>().Velocity = Quaternion.Euler(0, 0, angle) * toPlayer * speed;
        }
    }

    public void CheckActors(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        // get emitter
        StageData.Actor.Emitter em = current.DefaultEmitter;
        if (em == null)
        {
            throw new InvalidOperationException($"Timeline shoot_at_player action in actor {current.Name} in file {current.File} cannot be run an an actor without any emitters.");
        }
        if (Emitter != null)
        {
            if (!current.Emitters.ContainsKey(Emitter))
            {
                throw new InvalidOperationException($"Timeline shoot_at_player action in actor {current.Name} in file {current.File} uses emitter {Emitter} which does not exist.");
            }
            em = current.Emitters[Emitter];
        }
        else
        {
            Emitter = em.Name;
        }
        // attempt to use emitter's default actor
        if (Actor == null)
        {
            if (em?.Actor == null)
            {
                throw new InvalidOperationException($"Timeline spawn action in actor {current.Name} in file {current.File} is missing an 'actor' field.");
            }
            else
            {
                Actor = em.Actor;
            }
        }
        if (!actors.ContainsKey(Actor))
        {
            throw new InvalidOperationException($"Timeline shoot_at_player action in actor {current.Name} in file {current.File} attempts to shoot {Actor} which does not exist.");
        }
    }
}

public class MoveTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move";

    public float? X;
    public float? Y;
    public float? Dir;
    public float? Dist;
    public float? Speed;
    public bool? Instant;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        float speed = Speed ?? StageDirector.Instance.Data.Actors[actor.ActorType].Speed ?? 1;
        if (X != null || Y != null)
        {
            Vector2 diff = new Vector2(X ?? 0, Y ?? 0);
            Vector2 vel = diff.normalized * speed;
            float dur = diff.magnitude / speed;
            Vector2 target = (Vector2)runner.transform.position + diff;
            actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target));
        }
        else if (Dir != null)
        {
            Vector2 vel = Quaternion.Euler(0, 0, Dir.Value) * Vector2.right * speed;
            if (Dist != null)
            {
                float dur = Dist.Value / speed;
                Vector2 target = (Vector2)runner.transform.position + dur * vel;
                actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target));
            }
            else
            {
                actor.RunMoveCoroutine(actor.MoveCoroutine(vel));
            }
        }
        else
        {
            // stop if no args
            actor.RunMoveCoroutine(actor.MoveCoroutine(Vector2.zero));
        }
    }
}

public class MoveAbsTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move_abs";

    public float X;
    public float Y;
    public float? Speed;
    public bool? Instant;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveAbsTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        StageActor actor = runner.GetComponent<StageActor>();
        float speed = Speed ?? StageDirector.Instance.Data.Actors[actor.ActorType].Speed ?? 1;
        Vector2 target = new Vector2(X, Y);
        Vector2 diff = target - (Vector2)runner.transform.position;
        Vector2 vel = diff.normalized * speed;
        float dur = diff.magnitude / speed;
        actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target));
    }
}

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
        float speed = Speed ?? StageDirector.Instance.Data.Actors[actor.ActorType].Speed ?? 1;
        Vector2 target = PlayerController.Instance.transform.position;
        Vector2 diff = target - (Vector2)runner.transform.position;
        Vector2 vel = diff.normalized * speed;
        float dur = diff.magnitude / speed;
        actor.RunMoveCoroutine(actor.MoveCoroutine(vel, (Instant ?? false) ? 0 : dur, target));
    }
}


public class DestroyTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroy";
    public bool active = true;

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
}