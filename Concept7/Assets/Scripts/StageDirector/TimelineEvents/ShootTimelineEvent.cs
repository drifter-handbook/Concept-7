using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static StageDataUtils;

public class ShootTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
{
    public string Action => "shoot";

    public int? Num;
    public float Angle;
    public List<string> Emitters;
    public string Actor;
    public string Run;
    public float? Speed;
    public float? Dir;
    public float? Interval;
    public string Parent;
    public bool? MirrorX;
    public bool? MirrorY;
    public float? Lifetime;
    public string NumModifier;
    public string AngleModifier;
    public string LifetimeModifier;
    public string DirModifier;
    public string SpeedModifier;
    public bool Ring;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<ShootTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        actor.StartCoroutine(ShootCoroutine(actor));
    }

    IEnumerator ShootCoroutine(StageActor runnerActor)
    {
        // find emitter
        List<GameObject> em = Emitters.Select(x => runnerActor.Emitters[x]).ToList();
        float speed = (Speed ?? StageDirector.Instance.Data.Actors[Actor].Speed ?? 1) + GetVar(runnerActor, SpeedModifier);
        // find closest player
        List<StageActor> actorList = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<StageActor>()).Where(x => x != null).ToList();
        StageActor targetActor = NearestActor(actorList, runnerActor);
        List<Vector2> toTarget = null;
        if (targetActor != null)
        {
            toTarget = em.Select(x => ((Vector2)targetActor.transform.position - (Vector2)x.transform.position).normalized).ToList();
        }
        else
        {
            Dir = Dir ?? 0;
        }
        if (Dir != null)
        {
            toTarget = em.Select(x => (Vector2)(Quaternion.Euler(0f, 0f, Dir.Value + GetVar(runnerActor, DirModifier)) * Vector2.right)).ToList();
        }
        // create Num shots with Angle spread
        int shots = (Num ?? 1) + (int)GetVar(runnerActor, NumModifier) + (Ring ? 1 : 0);
        float time = 0f;
        // get/create parents for emitters
        List<GameObject> parent = null;
        if (Parent != null)
        {
            parent = Enumerable.Range(0, Emitters.Count).Select(i => GetParent(Parent, em[i].transform.position, runnerActor.gameObject, Emitters[i])).ToList();
        }
        float spread = Angle + GetVar(runnerActor, AngleModifier);
        float? lifetime = Lifetime + GetVar(runnerActor, LifetimeModifier);
        for (int i = 0; i < shots; i++)
        {
            if (Ring && i == shots - 1)
            {
                continue;
            }
            while (time < (Interval ?? 0) * i)
            {
                yield return null;
                time += Time.deltaTime;
            }
            float angle = 0f;
            if (shots > 1)
            {
                angle = Mathf.Lerp(spread * -0.5f, spread * 0.5f, (float)i / (shots - 1));
            }
            for (int j = 0; j < Emitters.Count; j++)
            {
                GameObject shot = StageDirector.Spawn(Actor, em[j].transform.position, 0f);
                StageActor actor = shot.GetComponent<StageActor>();
                float mirrorX = MirrorX == null ? runnerActor.Mirror.x : (MirrorX.Value ? -1 : 1);
                float mirrorY = MirrorY == null ? runnerActor.Mirror.y : (MirrorY.Value ? -1 : 1);
                actor.Direction = Quaternion.Euler(0, 0, angle) * toTarget[j];
                actor.Direction = new Vector2(actor.Direction.x * mirrorX, actor.Direction.y * mirrorY);
                actor.Speed = speed;
                actor.Mirror = new Vector2(mirrorX, mirrorY);
                if (parent != null)
                {
                    shot.transform.parent = parent[j].transform;
                }
                actor.FinishSpawn(Run, lifetime);
                foreach (var handler in runnerActor.gameObject.GetComponentsInChildren<IActorSpawnHandler>())
                {
                    handler.HandleSpawn(actor);
                }
            }
        }
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        // get emitter
        StageData.Actor.Emitter em = current.DefaultEmitter;
        if (em == null)
        {
            throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} cannot be run on an actor without any emitters.");
        }
        if (Emitters != null)
        {
            foreach (string e in Emitters)
            {
                if (!current.Emitters.ContainsKey(e))
                {
                    throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} uses emitter {e} which does not exist.");
                }
                em = current.Emitters[e];
            }
        }
        else
        {
            Emitters = new List<string>() { em.Name };
        }
        // attempt to use emitter's default actor
        if (Actor == null)
        {
            if (em?.Actor == null)
            {
                throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} is missing an 'actor' field.");
            }
            else
            {
                Actor = em.Actor;
            }
        }
        if (!actors.ContainsKey(Actor))
        {
            throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} attempts to shoot {Actor} which does not exist.");
        }
        if (Parent != null && !SpawnTimelineEvent.ParentValues.Contains(Parent.ToLower()))
        {
            throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} has 'parent' field {Parent} where the only allowed values are [null, 'new', 'actor', 'emitter']");
        }
        foreach (string v in new List<string> { NumModifier, AngleModifier, LifetimeModifier, DirModifier, SpeedModifier })
        {
            if (v != null && !current.Vars.ContainsKey(v))
            {
                throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} tries to use undefined variable {v}");
            }
        }
    }
}