using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class DestroyTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroy";
    public bool Active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return new DestroyTimelineEvent()
        {
            Active = Deserialize<bool>(actor, $"Timeline event {Action}", yaml)
        };
    }

    public void Start(MonoBehaviour runner)
    {
        if (Active)
        {
            if (runner.gameObject != null)
            {
                StageActor actor = runner.GetComponent<StageActor>();
                actor.RunTimeline(actor.Actor.OnDestroy?.Event);
                UnityEngine.Object.Destroy(runner.gameObject);
            }
        }
    }
}