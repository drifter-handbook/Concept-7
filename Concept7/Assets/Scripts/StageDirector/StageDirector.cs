using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageDirector : MonoBehaviour
{
    public StageData Data;
    public GameObject DefaultActorPrefab;
    public List<GameObject> Prefabs;

    // Singleton is a good design pattern, I swear
    public static StageDirector Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // load YAML data
        // if this is too much of a performance hit, load over a coroutine and set a flag when ready
        Data = new StageData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static GameObject StartStage(int i)
    {
        return Spawn(Instance.Data.Stages[i].Actor, Vector2.zero, 0f);
    }

    public static GameObject Spawn(string actor, Vector2 position, float rotation, string run=null)
    {
        // create actor
        GameObject actorObj = Instantiate(Instance.Data.Actors[actor].PrefabObj, new Vector3(position.x, position.y, Instance.Data.Actors[actor].Depth ?? 0), Quaternion.Euler(0f, 0f, rotation));
        // ensure StageActor component exists
        StageActor stActor = actorObj.GetComponent<StageActor>();
        if (stActor == null)
        {
            stActor = actorObj.AddComponent<StageActor>();
        }
        stActor.Initialize(actor);
        // use default timeline if possible
        if (run == null)
        {
            run = Instance.Data.Actors[actor].DefaultRun;
        }
        // run timeline
        if (run != null)
        {
            stActor.RunTimeline(run);
        }
        return actorObj;
    }
}