using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageActor : MonoBehaviour
{
    public Vector2 Velocity;

    public string ActorType;
    public Dictionary<string, GameObject> Emitters = new Dictionary<string, GameObject>();
    StageData.Actor Actor;

    // Only one movement coroutine allowed at a time.
    // Otherwise, they will fight and weird stuff will happen
    Coroutine movementCoroutine;

    public void Initialize(string actorType)
    {
        ActorType = actorType;
        Actor = StageDirector.Instance.Data.Actors[ActorType];
        // create emitters
        foreach (var em in StageDirector.Instance.Data.Actors[ActorType].Emitters)
        {
            GameObject go = new GameObject();
            go.transform.parent = gameObject.transform;
            go.transform.localPosition = new Vector2(em.Value.X, em.Value.Y);
            Emitters[em.Key] = go;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (Vector3)Velocity * Time.deltaTime;
        RefreshAngle();
    }

    public void RefreshAngle()
    {
        if (Actor?.TurnOnMove ?? false && Velocity != Vector2.zero)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Velocity));
        }
    }

    public void RunTimeline(string name)
    {
        var timelines = Actor.Timelines;
        if (!timelines.ContainsKey(name))
        {
            throw new InvalidOperationException($"Attempting to run timeline named {name} on actor {ActorType} but no such timeline exists.");
        }
        Debug.Log($"Running {name} with {timelines[name].Entries.Count} entries on actor {ActorType}");
        StartCoroutine(RunTimelineCoroutine(timelines[name]));
    }

    IEnumerator RunTimelineCoroutine(StageData.Actor.Timeline timeline)
    {
        float time = 0f;
        foreach (var entry in timeline.Entries)
        {
            while (time < entry.Time)
            {
                yield return null;
                time += Time.deltaTime;
            }
            Debug.Log($"Running entry at time {entry.Time} on actor {ActorType}");
            StartCoroutine(RunEntry(entry));
        }
    }

    IEnumerator RunEntry(StageData.Actor.Timeline.Entry entry)
    {
        int repeat = entry.Repeat ?? 1;
        float interval = entry.Interval ?? 0;
        float time = 0f;
        for (int i = 0; i < repeat; i++)
        {
            while (time < interval * i)
            {
                yield return null;
                time += Time.deltaTime;
            }
            entry.Event.Start(this);
        }
    }

    // enforce only one movement coroutine allowed at once
    public void RunMoveCoroutine(IEnumerator c)
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(c);
    }
    // move in a direction for a duration, or forever
    public IEnumerator MoveCoroutine(Vector2 vel, float? dur = null, Vector2? target = null)
    {
        Velocity = vel;
        if (dur == null)
        {
            while (true)
            {
                yield return null;
            }
        }
        for (float t = 0f; t < dur; t += Time.deltaTime)
        {
            yield return null;
        }
        Velocity = Vector2.zero;
        if (target != null)
        {
            transform.position = new Vector3(target.Value.x, target.Value.y, transform.position.z);
        }
    }
}
