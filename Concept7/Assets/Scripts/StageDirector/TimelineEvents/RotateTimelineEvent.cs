using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;
using System.Linq;

public class RotateTimelineEvent : StageData.Actor.Timeline.IEvent, StageData.Actor.ICompileCheck
{
    public string Action => "rotate";
    
    public float? Set;
    public float? Inc;
    public float? Dur;
    public float? Speed;
    public string Axis;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<RotateTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        float? angle = null;
        if (Set != null)
        {
            angle = actor.transform.position.z + Vector2.SignedAngle(Quaternion.Euler(0f, 0f, actor.transform.position.z) * Vector2.right, Quaternion.Euler(0f, 0f, Set ?? 0) * Vector2.right);
        }
        if (Inc != null)
        {
            angle = actor.transform.position.z + Inc.Value;
        }

        StageActor.Axis ax = StageActor.Axis.z;
        if (Axis != null)
        {
            ax = Enum.Parse<StageActor.Axis>(Axis);
        }
        actor.RunCoroutine(ref actor.rotateCoroutine, actor.RotateCoroutine(angle, Dur, Speed, ax));
    }

    public void CompileCheck(Dictionary<string, StageData.Actor> actors, StageData.Actor current)
    {
        List<string> axes = ((StageActor.Axis[])Enum.GetValues(typeof(StageActor.Axis))).Select(x => x.ToString()).ToList();
        if (Axis != null && !axes.Contains(Axis))
        {
            throw new StageDataException($"Timeline {Action} action contains field 'axis' with value {Axis}, but the value must be one of [{string.Join(", ", axes)}]");
        }
    }
}