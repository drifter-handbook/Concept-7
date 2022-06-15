using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

// Stores data about every actor file.
public partial class StageData
{
    public class Actor
    {
        public string File;
        public string Name;
        public List<string> CopyFrom;
        public string Prefab;
        public GameObject PrefabObj;
        public bool? Invuln;
        public int? Hp;
        public string DefaultRun;
        public bool? TurnOnMove;
        public List<string> Tags;
        public float? Speed;
        public float? Depth;
        public bool? DestroyOffscreen;
        public bool? DestroyOnImpact;
        public float? Lifetime;
        public string AttachOnImpact;
        public int? ActorLimit;
        public OnDestroyTimelines OnDestroy;
        public StageActor.ActorClassification? Classification;
        public Dictionary<string, Emitter> Emitters = new Dictionary<string, Emitter>();
        public Dictionary<string, Var> Vars = new Dictionary<string, Var>();
        public Dictionary<string, Timeline> Timelines = new Dictionary<string, Timeline>();

        public class OnDestroyTimelines
        {
            public string Offscreen;
            public string Impact;
            public string Event;
        }

        // TODO: DAG copy_from check
        public void EnsureCopy(Dictionary<string, Actor> actors, string copySource)
        {
            // handle copy_from
            if (!actors.ContainsKey(copySource))
            {
                throw new StageDataException($"Actor {Name} in file {File} attempts to copy_from {copySource} which does not exist.");
            }
            Actor copySrc = actors[copySource];
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
            if (DestroyOffscreen == null) { DestroyOffscreen = copySrc.DestroyOffscreen; }
            if (DestroyOnImpact == null) { DestroyOnImpact = copySrc.DestroyOnImpact; }
            if (Lifetime == null) { Lifetime = copySrc.Lifetime; }
            if (Classification == null) { Classification = copySrc.Classification; }
            if (AttachOnImpact == null) { AttachOnImpact = copySrc.AttachOnImpact; }
            if (ActorLimit == null) { ActorLimit = copySrc.ActorLimit; }
            if (OnDestroy == null) { OnDestroy = new OnDestroyTimelines(); }
            if (OnDestroy.Offscreen == null) { OnDestroy.Offscreen = copySrc.OnDestroy?.Offscreen; }
            if (OnDestroy.Impact == null) { OnDestroy.Impact = copySrc.OnDestroy?.Impact; }
            if (OnDestroy.Event == null) { OnDestroy.Event = copySrc.OnDestroy?.Event; }
            // copy over emitters, vars and timelines
            // The current object's take priority again
            foreach (string key in copySrc.Emitters.Keys)
            {
                if (!Emitters.ContainsKey(key))
                {
                    Emitters[key] = copySrc.Emitters[key];
                }
            }
            foreach (string key in copySrc.Vars.Keys)
            {
                if (!Vars.ContainsKey(key))
                {
                    Vars[key] = copySrc.Vars[key];
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
        public void Check(Dictionary<string, Actor> actors)
        {
            // check prefab exists
            if (Prefab != null)
            {
                if (!StageDirector.Instance.Prefabs.ContainsKey(Prefab))
                {
                    throw new StageDataException($"Actor {Name} in file {File} attempts to use prefab {Prefab} which is not registered with StageDirector.Prefabs");
                }
                PrefabObj = StageDirector.Instance.Prefabs[Prefab];
            }
            else
            {
                PrefabObj = StageDirector.Instance.DefaultActorPrefab;
            }
            // check attach exists
            if (AttachOnImpact != null && !actors.ContainsKey(AttachOnImpact))
            {
                throw new StageDataException($"Field 'attach_on_impact' {Name} in actor {Name} in file {File} attempts to use actor {AttachOnImpact} which does not exist.");
            }
            // check emitters and timelines for nonexistent actor types
            foreach (Emitter em in Emitters.Values)
            {
                em.CompileCheck(actors, this);
            }
            foreach (Timeline t in Timelines.Values)
            {
                foreach (Timeline.Entry entry in t.Entries)
                {
                    ICompileCheck check = entry.Event as ICompileCheck;
                    if (check != null)
                    {
                        check.CompileCheck(actors, this);
                    }
                }
                // make sure timelines are in sorted order
                t.Entries = t.Entries.OrderBy(x => x.Time).ToList();
            }
        }

        public Emitter DefaultEmitter => Emitters.Count > 0 ? Emitters.First().Value : null;

        public class Emitter : ICompileCheck
        {
            public string Name;
            public float X;
            public float Y;
            public string Actor;

            public void CompileCheck(Dictionary<string, Actor> actors, Actor current)
            {
                if (Actor != null && !actors.ContainsKey(Actor))
                {
                    throw new StageDataException($"Emitter {Name} in actor {current.Name} in file {current.File} attempts to emit {Actor} which does not exist.");
                }
            }
        }
        public class Var
        {
            public float Value;
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
                IEvent CloneFrom(Actor actor, string yaml);
                void Start(StageActor actor);
            }
        }
        // Checkable for actor references within actually existing
        public interface ICompileCheck
        {
            // throw if an actor within doesn't exist
            void CompileCheck(Dictionary<string, Actor> actors, Actor current);
        }
    }
}