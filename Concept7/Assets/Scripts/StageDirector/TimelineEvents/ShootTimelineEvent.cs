using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class ShootTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.IActorCheck
{
    public string Action => "shoot";

    public int? Num;
    public float Angle;
    public string Emitter;
    public string Actor;
    public string Run;
    public float? Speed;
    public float? Dir;
    public float? Interval;
    public string Parent;
    public bool? MirrorX;
    public bool? MirrorY;

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<ShootTimelineEvent>(yaml);
    }

    public void Start(MonoBehaviour runner)
    {
        runner.StartCoroutine(ShootCoroutine(runner));
    }

    IEnumerator ShootCoroutine(MonoBehaviour runner)
    {
        // find emitter
        StageActor runnerActor = runner.GetComponent<StageActor>();
        GameObject em = runnerActor.Emitters[Emitter];
        float speed = Speed ?? StageDirector.Instance.Data.Actors[Actor].Speed ?? 1;
        Vector2 toTarget = ((Vector2)PlayerController.Instance.transform.position - (Vector2)em.transform.position).normalized;
        if (Dir != null)
        {
            toTarget = Quaternion.Euler(0f, 0f, Dir.Value) * Vector2.right;
        }
        // create Num shots with Angle spread
        int shots = Num ?? 1;
        float time = 0f;
        // get/create parent
        GameObject parent = null;
        if (Parent != null)
        {
            parent = TimelineEventUtils.GetParent(Parent, em.transform.position, runner.gameObject, Emitter);
        }
        for (int i = 0; i < shots; i++)
        {
            while (time < (Interval ?? 0) * i)
            {
                yield return null;
                time += Time.deltaTime;
            }
            float angle = 0f;
            if (shots > 1)
            {
                angle = Mathf.Lerp(Angle * -0.5f, Angle * 0.5f, (float)i / (shots - 1));
            }
            GameObject shot = StageDirector.Spawn(Actor, em.transform.position, 0f);
            StageActor actor = shot.GetComponent<StageActor>();
            float mirrorX = MirrorX == null ? runnerActor.Mirror.x : (MirrorX.Value ? -1 : 1);
            float mirrorY = MirrorY == null ? runnerActor.Mirror.y : (MirrorY.Value ? -1 : 1);
            actor.Direction = Quaternion.Euler(0, 0, angle) * toTarget;
            actor.Direction = new Vector2(actor.Direction.x * mirrorX, actor.Direction.y * mirrorY);
            actor.Speed = speed;
            actor.Mirror = new Vector2(mirrorX, mirrorY);
            if (parent != null)
            {
                shot.transform.parent = parent.transform;
            }
            actor.RunTimeline(Run ?? StageDirector.Instance.Data.Actors[Actor].DefaultRun);
        }
    }

    public void CheckActors(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        // get emitter
        StageData.Actor.Emitter em = current.DefaultEmitter;
        if (em == null)
        {
            throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} cannot be run on an actor without any emitters.");
        }
        if (Emitter != null)
        {
            if (!current.Emitters.ContainsKey(Emitter))
            {
                throw new StageDataException($"Timeline shoot action in actor {current.Name} in file {current.File} uses emitter {Emitter} which does not exist.");
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
    }
}