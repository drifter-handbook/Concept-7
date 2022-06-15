using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// calls all other collision handlers
public class ActorCollisionCaller : MonoBehaviour
{
    public HashSet<GameObject> Exempt = new HashSet<GameObject>();
    HashSet<GameObject> NoLoopQueue = new HashSet<GameObject>();
    bool Ready;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!Ready && !NoLoopQueue.Contains(other.gameObject))
        {
            NoLoopQueue.Add(other.gameObject);
            return;
        }
        HandleCollide(other.gameObject);
    }

    void HandleCollide(GameObject other)
    {
        // only handle each collision once
        if (Exempt.Contains(other))
        {
            return;
        }
        foreach (IActorCollisionHandler handler in GetComponents<IActorCollisionHandler>())
        {
            if ((object)handler != this)
            {
                handler.HandleCollision(other);
            }
        }
        ExemptCollision(other);
    }

    public void ExemptCollision(GameObject target)
    {
        Exempt.Add(target);
    }

    // all exemptions copied over, and ready to handle new collisions
    public void SetAsReady()
    {
        Ready = true;
        foreach (GameObject go in NoLoopQueue)
        {
            HandleCollide(go);
        }
        NoLoopQueue.Clear();
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
}
