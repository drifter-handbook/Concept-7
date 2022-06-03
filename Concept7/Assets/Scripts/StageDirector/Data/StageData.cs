using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

// Loads and stores all data loaded from the StreamingAssets YAML files.
public partial class StageData
{
    // Info on "If Stage N starts, what actor should I create to run the stage?"
    public List<Stage> Stages = new List<Stage>();

    // Holds all actor types, and their data, emitters, and timelines
    public Dictionary<string, Actor> Actors = new Dictionary<string, Actor>();

    string WorkingDir => Application.streamingAssetsPath;
    const string StagesFile = "stages.yaml";

    const string EmitterPrefix = "emitter_";
    const string VarPrefix = "var_";
    const string TimelinePrefix = "timeline_";

    List<Actor.Timeline.IEvent> TimelineEvents = new List<Actor.Timeline.IEvent>()
    {
        new SpawnTimelineEvent(),
        new ShootTimelineEvent(),
        new MoveTimelineEvent(),
        new MoveAtPlayerTimelineEvent(),
        new DestroyTimelineEvent(),
        new SetSpeedTimelineEvent(),
        new OrbitTimelineEvent(),
        new SetVarTimelineEvent(),
        new RunTimelineTimelineEvent(),
        new RotateTimelineEvent(),
        new LinkTimelineEvent()
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
                throw new StageDataException($"Duplicate actor {actor.Name} in both {actor.File} and {Actors[actor.Name].File}");
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
        Dictionary<string, object> actorData = null;
        try
        {
            actorData = deserializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));
        }
        catch (YamlException e)
        {
            throw new StageDataException($"Failed to parse {path} as YAML: {e.Message}");
        }
        Debug.Log($"Parsing {path}");
        Actor actor = deserializer.Deserialize<Actor>(serializer.Serialize(actorData["core"]));
        actor.File = path;
        if (string.IsNullOrEmpty(actor.Name))
        {
            throw new StageDataException($"Failed to load {actor.File}: Missing actor name");
        }
        // load emitters and timelines
        foreach (string s in actorData.Keys)
        {
            // load emitters
            if (s.StartsWith(EmitterPrefix))
            {
                string name = s.Remove(s.IndexOf(EmitterPrefix), EmitterPrefix.Length);
                Actor.Emitter em = deserializer.Deserialize<Actor.Emitter>(serializer.Serialize(actorData[s]));
                em.Name = name;
                if (actor.Emitters.ContainsKey(em.Name))
                {
                    throw new StageDataException($"Duplicate emitter {em.Name} in {actor.File}");
                }
                actor.Emitters[em.Name] = em;
            }
            // load vars
            if (s.StartsWith(VarPrefix))
            {
                string name = s.Remove(s.IndexOf(VarPrefix), VarPrefix.Length);
                Actor.Var val = deserializer.Deserialize<Actor.Var>(serializer.Serialize(actorData[s]));
                actor.Vars[name] = val;
            }
            // load timelines
            if (s.StartsWith(TimelinePrefix))
            {
                string name = s.Remove(s.IndexOf(TimelinePrefix), TimelinePrefix.Length);
                Actor.Timeline timeline = new Actor.Timeline() { Name = name };
                if (actor.Timelines.ContainsKey(name))
                {
                    throw new StageDataException($"Duplicate timeline {name} in {actor.File}");
                }
                actor.Timelines[name] = timeline;
                foreach (Dictionary<object, object> evdata in (List<object>)actorData[s])
                {
                    // every event in the timeline must have exactly one "action" field, such as "move" or "shoot_at_player"
                    Dictionary<string, object> evconv = evdata.ToDictionary(x => (string)x.Key, x => x.Value);
                    Dictionary<string, object> ev = new Dictionary<string, object>(evconv);
                    if (!ev.ContainsKey("time"))
                    {
                        throw new StageDataException($"Timeline entry in timeline {name}, file {actor.File} contains no 'time' field.");
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
                            throw new StageDataException($"Timeline entry in timeline {name}, file {actor.File} contains more than one action: {actions[0]} and {actions[1]}");
                        }
                    }
                    if (ev2.Count == 0)
                    {
                        throw new StageDataException($"Timeline entry in timeline {name}, file {actor.File} contains no action.");
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
                                throw new StageDataException($"Action {action} matches more than one action in StageData.TimelineEvents!");
                            }
                            ste.Event = evt.CloneFrom(serializer.Serialize(ev2[action]), deserializer);
                        }
                    }
                    if (ste.Event == null)
                    {
                        throw new StageDataException($"Action {action} is not registered in StageData.TimelineEvents");
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
        foreach (var p in TopoSort())
        {
            Actor actor = Actors[p];
            if (actor.CopyFrom != null)
            {
                foreach (var c in actor.CopyFrom)
                {
                    actor.EnsureCopy(Actors, c);
                }
            }
        }
        foreach (var p in Actors)
        {
            p.Value.Check(Actors);
        }
    }

    // Port of https://en.wikipedia.org/wiki/Topological_sorting to C#
    private List<string> TopoSort()
    {
        List<string> order = new List<string>();
        HashSet<string> noPermMark = new HashSet<string>(Actors.Keys);
        HashSet<string> tempMark = new HashSet<string>();
        while (noPermMark.Count > 0)
        {
            TopoSortVisit(noPermMark.First(), noPermMark, tempMark, order);
        }
        return order;
    }
    private void TopoSortVisit(string n, HashSet<string> noPermMark, HashSet<string> tempMark, List<string> order)
    {
        if (!noPermMark.Contains(n))
        {
            return;
        }
        if (tempMark.Contains(n))
        {
            throw new StageDataException($"copy_from cycle found on {n}");
        }
        tempMark.Add(n);
        if (Actors[n].CopyFrom != null)
        {
            foreach (string m in Actors[n].CopyFrom)
            {
                if (!Actors.ContainsKey(m))
                {
                    throw new StageDataException($"Actor {Actors[n].Name} in file {Actors[n].File} attempts to copy_from {m} which does not exist.");
                }
                TopoSortVisit(m, noPermMark, tempMark, order);
            }
        }
        tempMark.Remove(n);
        noPermMark.Remove(n);
        order.Add(n);
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
}