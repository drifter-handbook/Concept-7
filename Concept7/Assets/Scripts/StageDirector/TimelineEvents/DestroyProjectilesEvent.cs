//using System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static StageDataUtils;

public class DestroyProjectilesEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "destroy_projectiles";
    public bool Active = true;

    public StageData.Actor.Timeline.IEvent CloneFrom(StageData.Actor actor, string yaml)
    {
        return new DestroyProjectilesEvent()
        {
            Active = Deserialize<bool>(actor, $"Timeline event {Action}", yaml)
        };
    }

    public void Start(MonoBehaviour runner)
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log(allEnemies.Length);
        for(int i = 0; i < allEnemies.Length; i++){
            Debug.Log(allEnemies[i].layer);
            // Enemy hitbox is layer 
            if (allEnemies[i].layer == 8) {
                UnityEngine.Object.Destroy(allEnemies[i]);
            }
        }
    }

}