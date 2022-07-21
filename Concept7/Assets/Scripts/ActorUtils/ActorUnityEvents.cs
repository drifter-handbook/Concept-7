using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActorUnityEvents : MonoBehaviour
{
    public List<ActorEvent> Events;

    [Serializable]
    public class ActorEvent
    {
        public UnityEvent Event;
        public string Name;
    }

    public bool Call(string name)
    {
        bool found = false;
        foreach (ActorEvent ev in Events)
        {
            if (ev.Name == name)
            {
                ev.Event.Invoke();
                found = true;
            }
        }
        return found;
    }
}
