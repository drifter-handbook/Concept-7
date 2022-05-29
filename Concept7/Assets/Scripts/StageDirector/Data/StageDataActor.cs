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
        public bool? DestroyOffscreen;
        public bool DestroyOnImpact;
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
                if (!StageDirector.Instance.Prefabs.ContainsKey(Prefab))
                {
                    throw new InvalidOperationException($"Actor {Name} in file {File} attempts to use prefab {Prefab} which is not registered with StageDirector.Prefabs");
                }
                PrefabObj = StageDirector.Instance.Prefabs[Prefab];
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