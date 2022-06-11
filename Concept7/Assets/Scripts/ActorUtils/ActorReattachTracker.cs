using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorReattachTracker : MonoBehaviour
{
    GameObject Parent;
    // spawn filler game object so actor group doesn't self-destruct
    GameObject Filler;

    public void Detach()
    {
        Filler = new GameObject($"{name} (Filler)");
        Filler.transform.parent = transform.parent;
        Parent = transform.parent.gameObject;
        transform.parent = null;
    }

    public void Reattach()
    {
        transform.parent = Parent.transform;
        Destroy(Filler);
        Destroy(this);
    }

    void OnDestroy()
    {
        Destroy(Filler);
    }
}
