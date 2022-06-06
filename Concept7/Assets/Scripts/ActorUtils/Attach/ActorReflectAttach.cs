using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorReflectAttach : MonoBehaviour, IActorAttachment, IActorSpawnHandler
{
    public List<StageActor.ActorClassification> Classifications;

    public WeaponData ReflectWeaponData;

    public void Attach(StageActor target, StageActor source)
    {
        // reflect target if actor is correct classification
        if (Classifications.Contains(target.Classification))
        {
            // set to be owned by the player
            target.gameObject.tag = "PlayerWeapon";
            target.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
            target.gameObject.AddComponent<PlayerWeapon>().weaponData = ReflectWeaponData;
            // set direction and stop timelines
            target.StopAllTimelines();
            // if too slow or no direction
            Vector2 dir = (target.transform.position - source.transform.position).normalized;
            // DebugDotter.Clear();
            // DebugDotter.Dot(actor.transform.position, Color.red);
            if (target.Speed < 0.1f || target.Direction == Vector2.zero)
            {
                target.Direction = dir;
            }
            else
            {
                // use velocity and rule that angle of incidence == angle of reflection
                // DebugDotter.Dot((Vector2)actor.transform.position - actor.Direction * 2f, Color.magenta);
                float incidence = Vector2.SignedAngle(-target.Direction, new Vector2(-dir.y, dir.x));
                // DebugDotter.Dot((Vector2)actor.transform.position + (new Vector2(-dir.y, dir.x) * 2f), Color.green);
                target.Direction = (Quaternion.Euler(0f, 0f, incidence) * new Vector2(dir.y, -dir.x)).normalized;
                // DebugDotter.Dot((Vector2)actor.transform.position + (new Vector2(dir.y, -dir.x) * 2f), Color.cyan);
                // Debug.Log(incidence);
            }
            // DebugDotter.Dot((Vector2)actor.transform.position + (actor.Direction * 2f), Color.blue);
            target.Speed = Mathf.Max(target.Speed, 4f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HandleSpawn(StageActor newActor)
    {
        // any spawned actors are also the player's
        newActor.gameObject.tag = "PlayerWeapon";
        newActor.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
    }
}
