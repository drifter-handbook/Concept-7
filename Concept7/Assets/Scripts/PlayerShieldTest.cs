using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This is a TEMPORARY TECH DEMO for reflecting bullets.
// This *better* not make into the final release, or else
// I'm going to have some choice words to say to, uh, myself I guess

// because this is a singleton, this does NOT support multiplayer.

public class PlayerShieldTest : MonoBehaviour
{
    public static PlayerShieldTest Instance;

    public GameObject PlayerObj;

    SpriteRenderer shieldSr;
    Collider2D shieldCol;
    Collider2D playerCol;

    public WeaponData ReflectWeaponData;

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
        shieldSr = GetComponent<SpriteRenderer>();
        shieldCol = GetComponent<Collider2D>();
        playerCol = PlayerObj.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Game.Instance.Shield)
        {
            playerCol.enabled = false;
            shieldSr.enabled = true;
            shieldCol.enabled = true;
        }
        else
        {
            playerCol.enabled = true;
            shieldSr.enabled = false;
            shieldCol.enabled = false;
        }
    }

    void Reflect(StageActor actor)
    {
        actor.StopAllTimelines();
        // if too slow or no direction
        Vector2 dir = (actor.transform.position - transform.position).normalized;
        // DebugDotter.Clear();
        // DebugDotter.Dot(actor.transform.position, Color.red);
        if (actor.Speed < 0.1f || actor.Direction == Vector2.zero)
        {
            actor.Direction = dir;
        }
        else
        {
            // use velocity and rule that angle of incidence == angle of reflection
            // DebugDotter.Dot((Vector2)actor.transform.position - actor.Direction * 2f, Color.magenta);
            float incidence = Vector2.SignedAngle(-actor.Direction, new Vector2(-dir.y, dir.x));
            // DebugDotter.Dot((Vector2)actor.transform.position + (new Vector2(-dir.y, dir.x) * 2f), Color.green);
            actor.Direction = (Quaternion.Euler(0f, 0f, incidence) * new Vector2(dir.y, -dir.x)).normalized;
            // DebugDotter.Dot((Vector2)actor.transform.position + (new Vector2(dir.y, -dir.x) * 2f), Color.cyan);
            // Debug.Log(incidence);
        }
        // DebugDotter.Dot((Vector2)actor.transform.position + (actor.Direction * 2f), Color.blue);
        actor.Speed = Mathf.Max(actor.Speed, 4f);
        SetToPlayerBullet(actor);
    }

    public static void SetToPlayerBullet(StageActor actor)
    {
        actor.gameObject.tag = "PlayerWeapon";
        actor.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
        actor.gameObject.AddComponent<PlayerWeapon>().weaponData = Instance.ReflectWeaponData;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsEnemyBullet(other.gameObject))
        {
            Reflect(other.GetComponent<StageActor>());
        }
    }

    bool IsEnemyBullet(GameObject obj)
    {
        return obj.tag == "Enemy" && (obj.GetComponent<ActorUseHP>() == null || obj.name.ToLower().Contains("missile"));
    }
}
