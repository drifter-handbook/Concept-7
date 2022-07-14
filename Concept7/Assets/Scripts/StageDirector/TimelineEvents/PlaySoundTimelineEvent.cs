using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class PlaySoundTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "playsound";
    
    public string Sound;
    public float? Volume;
    public bool? RandomizePitch;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return Deserialize<PlaySoundTimelineEvent>(actor, $"Timeline event {Action}", yaml);
    }

    public void Start(StageActor actor)
    {
        Game.Instance.PlaySFX(Sound, 0.1f * (Volume ?? 1f), (RandomizePitch ?? false) ? UnityEngine.Random.Range(0.5f, 1) : 1);
    }
}