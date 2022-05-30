using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// MonoBehaviour which controls spawned Actor GameObjects.
// Controlled by running timelines via RunTimeline(name).
public class StageActor : MonoBehaviour
{
    public float Speed;
    // normalized last nonzero velocity
    public Vector2 Direction;

    public string ActorType;
    public Dictionary<string, GameObject> Emitters = new Dictionary<string, GameObject>();
    StageData.Actor Actor;

    // Only one movement coroutine allowed at a time.
    // Otherwise, they will fight and weird stuff will happen
    Coroutine movementCoroutine;
    Coroutine speedCoroutine;

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
        // init vars
        Speed = Actor.Speed ?? 1f;
        if (Actor.DestroyOffscreen ?? true)
        {
            var comp = GetComponent<ActorDestroyOffscreen>() ?? gameObject.AddComponent<ActorDestroyOffscreen>();
        }
        if (Actor.DestroyOnImpact)
        {
            var comp = GetComponent<ActorDestroyOnImpact>() ?? gameObject.AddComponent<ActorDestroyOnImpact>();
        }
        if (!(Actor.Invuln ?? false))
        {
            var comp = GetComponent<ActorUseHP>() ?? gameObject.AddComponent<ActorUseHP>();
            comp.Initialize(Actor);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (movementCoroutine == null)
        {
            transform.position += (Vector3)Direction * Speed * Time.deltaTime;
            RefreshAngle();
        }
    }

    public void RefreshAngle()
    {
        if (Actor?.TurnOnMove ?? false)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
        }
    }

    public void RunTimeline(string name)
    {
        var timelines = Actor.Timelines;
        if (!timelines.ContainsKey(name))
        {
            throw new StageDataException($"Attempting to run timeline named {name} on actor {ActorType} but no such timeline exists.");
        }
        // Debug.Log($"Running {name} with {timelines[name].Entries.Count} entries on actor {ActorType}");
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
            // Debug.Log($"Running entry at time {entry.Time} on actor {ActorType}");
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

    // enforce only one speed coroutine allowed at once
    public void RunSpeedCoroutine(IEnumerator c)
    {
        if (speedCoroutine != null)
        {
            StopCoroutine(speedCoroutine);
        }
        speedCoroutine = StartCoroutine(c);
    }
    public IEnumerator SpeedCoroutine(float speed, float dur)
    {
        float time = 0f;
        float startSpd = Speed;
        while (time < dur)
        {
            Speed = Mathf.Lerp(startSpd, speed, time / dur);
            time += Time.deltaTime;
            yield return null;
        }
        Speed = speed;
    }

    // enforce only one movement coroutine allowed at once
    public void RunMoveCoroutine(IEnumerator c)
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        if (c != null)
        {
            movementCoroutine = StartCoroutine(c);
        }
    }
    // move in a direction for a duration, or forever
    const int ARCLEN_SEGMENTS = 7;
    const float SPLINE_TOOCLOSE = 0.01f;
    public IEnumerator MoveCoroutine(List<MoveTarget> spline, Vector2? finishdir=null, bool loop=false)
    {
        spline = new List<MoveTarget>(spline);
        // Remove keypoints too close together
        for (int i = 0; i < spline.Count - 1; i++)
        {
            if ((spline[i].Pos - spline[i + 1].Pos).magnitude < SPLINE_TOOCLOSE)
            {
                spline[i].Post = spline[i + 1].Post;
                spline.RemoveAt(i + 1);
                i--;
            }
        }
        // add loop at end, if not close enough starting point
        if (loop && spline.Count > 1 && (spline[spline.Count - 1].Pos - spline[1].Pos).magnitude >= SPLINE_TOOCLOSE)
        {
            MoveTarget first = new MoveTarget()
            {
                Pos = spline[1].Pos,
                Pre = spline[1].Pre,
                Post = spline[1].Post
            };
            first.Pre = first.Pos - (first.Post - first.Pos);
            spline[1] = first;
            MoveTarget last = spline[spline.Count - 1];
            spline[spline.Count - 1].Post = last.Pos + (last.Pos - last.Pre);
            spline.Add(first);
        }
        float dist = 0f;
        // run the spline
        for (int i = 0; i < spline.Count - 1; i++)
        {
            // arc-length parameterizing
            List<Vector2> samples = new List<Vector2>();
            samples.Add(spline[i].Pos);
            for (int j = 1; j < ARCLEN_SEGMENTS; j++)
            {
                samples.Add(CubicBezier(spline[i], spline[i + 1], (float)j/ARCLEN_SEGMENTS));
            }
            samples.Add(spline[i + 1].Pos);
            List<float> curveSpd = new List<float>();
            for (int j = 0; j < samples.Count - 1; j++)
            {
                curveSpd.Add((samples[j + 1] - samples[j]).magnitude);
            }
            float totalDist = curveSpd.Sum();
            // move along the curve
            float t = SpeedLerp(curveSpd, dist);
            while (t < 1)
            {
                dist += Speed * Time.deltaTime;
                t = SpeedLerp(curveSpd, dist);
                Vector2 pos = CubicBezier(spline[i], spline[i + 1], t);
                transform.position = new Vector3(pos.x, pos.y, transform.position.z);
                Direction = (CubicBezier(spline[i], spline[i + 1], t + 0.01f) - pos).normalized;
                yield return null;
            }
            dist -= totalDist;
            // at the end, loop if necessary
            if (loop && i == spline.Count - 2)
            {
                i = 0;
            }
        }
        // set finishdir if needed, for "move infinitely in direction"
        if (finishdir != null)
        {
            Direction = finishdir.Value;
        }
        movementCoroutine = null;
    }
    Vector2 CubicBezier(MoveTarget cur, MoveTarget next, float t)
    {
        Vector2 v = Vector2.Lerp(cur.Post, next.Pre, t);
        Vector2 a = Vector2.Lerp(Vector2.Lerp(cur.Pos, cur.Post, t), v, t);
        Vector2 b = Vector2.Lerp(v, Vector2.Lerp(next.Pre, next.Pos, t), t);
        return Vector2.Lerp(a, b, t);
    }
    float SpeedLerp(List<float> curveSpd, float dist)
    {
        for (int i = 0; i < curveSpd.Count; i++)
        {
            if (dist <= curveSpd[i])
            {
                return Mathf.Lerp((float)i / curveSpd.Count, (float)(i + 1) / curveSpd.Count, dist / curveSpd[i]);
            }
            dist -= curveSpd[i];
        }
        return 1f;
    }

    // target in world space
    public class MoveTarget
    {
        public Vector2 Pos;
        public Vector2 Pre;
        public Vector2 Post;
    }
}
