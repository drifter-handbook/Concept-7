using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// calls all other collision handlers
public class ActorCollisionCaller : MonoBehaviour
{
    public HashSet<GameObject> Exempt = new HashSet<GameObject>();
    HashSet<GameObject> CollisionQueue = new HashSet<GameObject>();
    public bool Ready;
    public bool Jitter = false;
    public bool ColorJitter = false;
    public float JitterAmt = 5;
    public float JitterTime = 0.2f;

    public List<StageActor.ActorClassification> Classifications = new List<StageActor.ActorClassification>();

    void Start()
    {
        StartCoroutine(SetAsReady());
        if (Classifications.Count == 0)
        {
            foreach (StageActor.ActorClassification cl in Enum.GetValues(typeof(StageActor.ActorClassification)))
            {
                Classifications.Add(cl);
            }
        }
    }

    void Update()
    {
        foreach (GameObject go in CollisionQueue)
        {
            if (gameObject == null)
            {
                break;
            }
            if (go == null)
            {
                continue;
            }
            HandleCollide(go);
        }
        CollisionQueue.Clear();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // only handle each collision once
        if (Exempt.Contains(other.gameObject))
        {
            return;
        }
        CollisionQueue.Add(other.gameObject);
    }

    void HandleCollide(GameObject other)
    {
        if (gameObject == null || other == null)
        {
            return;
        }
        // only handle each collision once
        if (Exempt.Contains(other))
        {
            return;
        }
        StageActor otherActor = other.GetComponent<StageActor>();
        if (otherActor == null || !Classifications.Contains(otherActor.Classification))
        {
            return;
        }
        foreach (IActorCollisionHandler handler in GetComponents<IActorCollisionHandler>())
        {
            if ((object)handler != this || other == null)
            {
                handler.HandleCollision(other);
            }
        }
        Exempt.Add(other);
        
        if(Jitter){
            StartCoroutine(JitterSprite());
        }
    }

    public void ExemptCollision(GameObject target)
    {
        Exempt.Add(target);
    }

    IEnumerator SetAsReady()
    {
        yield return null;
        Ready = true;
    }

    public static void SetExempt(GameObject a, GameObject b)
    {
        // exempt a from colliding with b
        ActorCollisionCaller caller = a.GetComponent<ActorCollisionCaller>();
        if (caller != null)
        {
            caller.ExemptCollision(b);
        }
        // exempt b from colliding with a
        caller = b.GetComponent<ActorCollisionCaller>();
        if (caller != null)
        {
            caller.ExemptCollision(a);
        }
    }

    IEnumerator JitterSprite(){
        //Jostle the sprite a little when hit
        Material hitMaterial = GetComponent<SpriteRenderer>().material;
        if(ColorJitter){
             hitMaterial.SetColor("_HitColor", Color.red/2 + Color.yellow/8);
        }
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z-JitterAmt);
        yield return new WaitForSeconds(JitterTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z+(JitterAmt*2));
         yield return new WaitForSeconds(JitterTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z-JitterAmt);
        hitMaterial.SetColor("_HitColor", Color.black);
    }
}
