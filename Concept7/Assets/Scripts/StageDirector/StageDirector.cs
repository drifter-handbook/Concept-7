using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

// See stagedirector.md
// Singleton data store which loads YAML from StreamingAssets,
// and allows reference of any prefabs in Resources/Prefabs by name.
// Allows for spawning in actors, or finding weapons by RYB count.
public class StageDirector : MonoBehaviour
{
    public StageData Data;
    public GameObject DefaultActorPrefab;
    public Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
    public List<StageActor> CurrentActors = new List<StageActor>();
    // used in FindWeapon
    Dictionary<string, string> Weapons = new Dictionary<string, string>();
    Dictionary<string, StageData.Actor> WeaponsActor = new Dictionary<string, StageData.Actor>();

    public Dictionary<string, int> ActorCount = new Dictionary<string, int>();

    public GameObject Stage;

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
        // load all prefabs
        if (Prefabs.Count == 0)
        {
            foreach (GameObject prefab in Resources.LoadAll<GameObject>("Prefabs"))
            {
                if (prefab)
                {
                    if (Prefabs.ContainsKey(prefab.name))
                    {
                        throw new StageDataException($"Failed to load prefab {prefab.name}: another prefab has the same name in Resources/Prefabs");
                    }
                    Prefabs[prefab.name] = prefab;
                    // check for weapon, if weapon, populate the thing
                    WeaponData weap = prefab.GetComponent<PlayerWeapon>()?.weaponData;
                    if (weap != null)
                    {
                        Weapons[RYBStr(weap.r, weap.y, weap.b)] = prefab.name;
                    }
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Load();
    }

    void Load()
    {
        // load YAML data
        // if this is too much of a performance hit, load over a coroutine and set a flag when ready
        Data = new StageData();
    }

    // Update is called once per frame
    void Update()
    {
        // remove destroyed actors
        for (int i = 0; i < CurrentActors.Count; i++)
        {
            if (CurrentActors[i] == null)
            {
                CurrentActors.RemoveAt(i);
                i--;
            }
        }
    }

    public static StageData.Actor FindWeapon(string ryb)
    {
        if (!Instance.Weapons.ContainsKey(ryb))
        {
            Debug.Log($"Warning! No weapon exists with combo {ryb}");
            return null;
        }
        // find prefab for actor type
        string prefabname = Instance.Weapons[ryb];
        // find actor for prefab
        if (Instance.WeaponsActor.ContainsKey(prefabname))
        {
            return Instance.WeaponsActor[prefabname];
        }
        foreach (var actor in Instance.Data.Actors.Values)
        {
            if (actor.Prefab == prefabname)
            {
                Instance.WeaponsActor[prefabname] = actor;
            }
        }
        if (!Instance.WeaponsActor.ContainsKey(prefabname))
        {
            throw new StageDataException($"Prefab exists but actor for combo {ryb} does not exist.");
        }
        return Instance.WeaponsActor[prefabname];
    }

    static string[] rybOrder = { "R", "Y", "B" };
    public static string RYBStr(int r, int y, int b)
    {
        string s = "";
        int[] ryb = {r, y, b};
        for (int i = 0; i < rybOrder.Length; i++)
        {
            while (ryb[i] != 0)
            {
                s += ryb[i] > 0 ? rybOrder[i] : rybOrder[i].ToLower();
                ryb[i] = (int)Mathf.Round(Mathf.MoveTowards(ryb[i], 0, 1));
            }
        }
        return s;
    }

    public static GameObject StartStage(int i)
    {
        StageData.Actor actor = Instance.Data.Actors[Instance.Data.Stages[i].Actor];
        Instance.Stage = Spawn(actor.Name, Vector2.zero, 0f);
        Instance.Stage.GetComponent<StageActor>().FinishSpawn(null);
        return Instance.Stage;
    }

    public static bool IsStageFinished()
    {
        if (Instance.Stage == null || Instance.Stage.GetComponent<StageActor>().RunningTimelines.Count == 0)
        {
            return GameObject.FindGameObjectsWithTag("Enemy").Length == 0;
        }
        return false;
    }

    public static GameObject FindCurrentActor(string actorType)
    {
        foreach (StageActor actor in Instance.CurrentActors)
        {
            if (actor != null && actor.ActorType == actorType)
            {
                return actor.gameObject;
            }
        }
        return null;
    }

    public static GameObject Spawn(string actor, Vector2 position, float rotation)
    {
        // check actor limit
        int limit = Instance.Data.Actors[actor].ActorLimit ?? 0;
        if (limit > 0)
        {
            if (!Instance.ActorCount.ContainsKey(actor))
            {
                Instance.ActorCount[actor] = 0;
            }
            else if (Instance.ActorCount[actor] > limit)
            {
                return null;
            }
            Instance.ActorCount[actor]++;
        }
        // create actor
        GameObject actorObj = Instantiate(Instance.Data.Actors[actor].PrefabObj);
        actorObj.transform.localPosition = new Vector3(position.x, position.y, Instance.Data.Actors[actor].Depth ?? 0);
        actorObj.transform.localEulerAngles = new Vector3(actorObj.transform.localEulerAngles.x, actorObj.transform.localEulerAngles.y, rotation);
        // ensure StageActor component exists
        StageActor stActor = actorObj.GetComponent<StageActor>();
        if (stActor == null)
        {
            stActor = actorObj.AddComponent<StageActor>();
        }
        Instance.CurrentActors.Add(stActor);
        stActor.Initialize(actor);
        return actorObj;
    }
}
