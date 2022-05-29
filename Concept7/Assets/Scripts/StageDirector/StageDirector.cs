using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// See stagedirector.md
// Singleton data store which loads YAML from StreamingAssets,
// and allows reference of any prefabs in Resources/Prefabs by name.
// Allows for spawning in actors, or finding weapons by RYB count.
public class StageDirector : MonoBehaviour
{
    public StageData Data;
    public GameObject DefaultActorPrefab;
    public Dictionary<string, GameObject> Prefabs = new Dictionary<string, GameObject>();
    // used in FindWeapon
    Dictionary<string, string> Weapons = new Dictionary<string, string>();
    Dictionary<string, StageData.Actor> WeaponsActor = new Dictionary<string, StageData.Actor>();

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
                        throw new InvalidOperationException($"Failed to load prefab {prefab.name}: another prefab has the same name in Resources/Prefabs");
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

    }

    public static StageData.Actor FindWeapon(int r, int y, int b)
    {
        string k = RYBStr(r, y, b);
        if (!Instance.Weapons.ContainsKey(k))
        {
            Debug.Log($"Warning! No weapon exists with combo R{r}Y{y}B{b} ({k})");
            return null;
        }
        // find prefab for actor type
        string prefabname = Instance.Weapons[k];
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
            throw new InvalidOperationException($"Prefab exists but actor for combo combo R{r}Y{y}B{b} ({k}) does not exist.");
        }
        return Instance.WeaponsActor[prefabname];
    }

    static string[] rybOrder = { "R", "Y", "B" };
    static string RYBStr(int r, int y, int b)
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