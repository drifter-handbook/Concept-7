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
        GameObject em = runner.GetComponent<StageActor>().Emitters[Emitter];
        float speed = Speed ?? StageDirector.Instance.Data.Actors[Actor].Speed ?? 1;
        Vector2 toTarget = ((Vector2)PlayerController.Instance.transform.position - (Vector2)em.transform.position).normalized;
        if (Dir != null)
        {
            toTarget = Quaternion.Euler(0f, 0f, Dir.Value) * Vector2.right;
        }
        // create Num shots with Angle spread
        int shots = Num ?? 1;
        float time = 0f;
        for (int i = 0; i < shots; i++)
        {
            while (time < (Interval ?? 0) * i)
            {
                yield return null;
                time += Time.deltaTime;
            }
            float angle = Mathf.Lerp(Angle * -0.5f, Angle * 0.5f, (float)i / shots);
            GameObject shot = StageDirector.Spawn(Actor, em.transform.position, 0f, Run);
            shot.GetComponent<StageActor>().Velocity = Quaternion.Euler(0, 0, angle) * toTarget * speed;
        }
    }

    public void CheckActors(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        // get emitter
        StageData.Actor.Emitter em = current.DefaultEmitter;
        if (em == null)
        {
            throw new InvalidOperationException($"Timeline shoot action in actor {current.Name} in file {current.File} cannot be run on an actor without any emitters.");
        }
        if (Emitter != null)
        {
            if (!current.Emitters.ContainsKey(Emitter))
            {
                throw new InvalidOperationException($"Timeline shoot action in actor {current.Name} in file {current.File} uses emitter {Emitter} which does not exist.");
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
                throw new InvalidOperationException($"Timeline shoot action in actor {current.Name} in file {current.File} is missing an 'actor' field.");
            }
            else
            {
                Actor = em.Actor;
            }
        }
        if (!actors.ContainsKey(Actor))
        {
            throw new InvalidOperationException($"Timeline shoot action in actor {current.Name} in file {current.File} attempts to shoot {Actor} which does not exist.");
        }
    }
}