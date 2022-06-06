using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorAttachOnImpact : MonoBehaviour
{
    HashSet<GameObject> hits = new HashSet<GameObject>();
    public string AttachActor;

    private void OnTriggerEnter2D(Collider2D other)
    {
    	if(other.gameObject.tag != "PlayArea")
        {
            if (AttachActor != null && !hits.Contains(other.gameObject))
            {
                StageActor target = other.gameObject.GetComponent<StageActor>();
                if (target != null)
                {
                    GameObject go = StageDirector.Spawn(AttachActor, new Vector3(target.transform.position.x, target.transform.position.y), 0f);
                    go.GetComponent<StageActor>().FinishSpawn();
                    foreach (var handler in go.GetComponentsInChildren<IActorAttachment>())
                    {
                        handler.Attach(target, GetComponent<StageActor>());
                    }
                }
                hits.Add(other.gameObject);
            }
        }
    }
}
