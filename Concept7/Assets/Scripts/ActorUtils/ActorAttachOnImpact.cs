using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ActorCollisionCaller))]
public class ActorAttachOnImpact : MonoBehaviour, IActorCollisionHandler
{
    public string AttachActor;

    public void HandleCollision(GameObject other)
    {
        if (gameObject.tag == "PlayArea")
        {
            return;
        }
        if (AttachActor == null)
        {
            return;
        }
        StageActor target = other.GetComponent<StageActor>();
        if (target != null)
        {
            GameObject go = StageDirector.Spawn(AttachActor, new Vector3(target.transform.position.x, target.transform.position.y), 0f);
            StageActor spawner = GetComponent<StageActor>();
            if (go != null)
            {
                go.GetComponent<StageActor>().FinishSpawn(spawner);
            }
            if (go != null)
            {
                foreach (var handler in go.GetComponentsInChildren<IActorAttachment>())
                {
                    if (go == null)
                    {
                        break;
                    }
                    handler.Attach(target, spawner);
                }
            }
        }
    }
}
