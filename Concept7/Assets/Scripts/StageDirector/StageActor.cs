using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// MonoBehaviour which controls spawned Actor GameObjects.
// Controlled by running timelines via RunTimeline(name).
public class StageActor : MonoBehaviour, IActorDestroyHandler
{
    public float Speed;
    // normalized last nonzero velocity
    public Vector2 Direction;
    // angular velocity only used for the orbit command.
    public float OrbitSpeed;
    public float? OrbitRadius;
    public Quaternion OrbitTilt = Quaternion.Euler(Vector3.zero);
    public float? OrbitAngle;

    public string ActorType;
    public Dictionary<string, GameObject> Emitters = new Dictionary<string, GameObject>();
    public Dictionary<string, float> Vars = new Dictionary<string, float>();
    public StageData.Actor Actor;

    public ActorClassification Classification = ActorClassification.Actor;
    // how should this actor be treated by things that intercept/reflect projectiles?
    [Serializable]
    public enum ActorClassification
    {
        Actor, Bullet, Beam, Missile
    }

    // Only one movement coroutine allowed at a time.
    // Otherwise, they will fight and weird stuff will happen
    public Coroutine movementCoroutine;
    public Coroutine speedCoroutine;
    public Coroutine orbitSpeedCoroutine;
    public Coroutine orbitRadiusCoroutine;
    public Coroutine orbitTiltCoroutine;
    public Coroutine rotateCoroutine;

    int CurrentTimelineID = 0;
    int EventID = 0;
    public List<RunningTimeline> RunningTimelines = new List<RunningTimeline>();
    public class RunningTimeline
    {
        public int ID;
        public string Name;
        public Coroutine Coroutine;
    }
    public class RunningEvent : IComparable<RunningEvent>
    {
        public float Time;
        public int EventID;
        public int RepeatIndex;
        public int CompareTo(RunningEvent other)
        {
            int timeCmp = Time.CompareTo(other.Time);
            if (timeCmp == 0)
            {
                int rpCmp = RepeatIndex.CompareTo(other.RepeatIndex);
                if (rpCmp == 0)
                {
                    return EventID.CompareTo(other.EventID);
                }
                return rpCmp;
            }
            return timeCmp;
        }
    }

    // mirroring X/Y
    public Vector2 Mirror = Vector2.one;

    public void Initialize(string actorType)
    {
        ActorType = actorType;
        Actor = StageDirector.Instance.Data.Actors[ActorType];
        // create emitters
        foreach (var em in Actor.Emitters)
        {
            GameObject go = new GameObject();
            go.transform.parent = gameObject.transform;
            go.transform.localPosition = new Vector2(em.Value.X, em.Value.Y);
            go.name = $"emitter_{em.Key}";
            Emitters[em.Key] = go;
        }
        // copy vars
        foreach (var v in Actor.Vars)
        {
            Vars[v.Key] = v.Value.Value;
        }
        // init core fields
        Speed = Actor.Speed ?? 1f;
        if (Actor.DestroyOffscreen ?? true)
        {
            var comp = GetComponent<ActorDestroyOffscreen>() ?? gameObject.AddComponent<ActorDestroyOffscreen>();
        }
        if (Actor.DestroyOnImpact ?? false)
        {
            var comp = GetComponent<ActorDestroyOnImpact>() ?? gameObject.AddComponent<ActorDestroyOnImpact>();
        }
        if (!(Actor.Invuln ?? false))
        {
            var comp = GetComponent<ActorUseHP>() ?? gameObject.AddComponent<ActorUseHP>();
            comp.Initialize(Actor);
        }
        Classification = Actor.Classification ?? Classification;
        if (Actor.AttachOnImpact != null)
        {
            var comp = GetComponent<ActorAttachOnImpact>() ?? gameObject.AddComponent<ActorAttachOnImpact>();
            comp.AttachActor = Actor.AttachOnImpact;
        }
        // stage editor stuff
        HandleStageEditorSpawn();
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
            transform.localPosition += (Vector3)Direction * Speed * Time.deltaTime;
            OrbitTilt = Quaternion.Euler(Vector3.zero);
            OrbitAngle = null;
            OrbitRadius = null;
            RefreshAngle();
        }
    }

    public void RefreshAngle()
    {
        if (Actor?.TurnOnMove ?? false && rotateCoroutine == null)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, Vector2.SignedAngle(Vector2.right, Direction));
        }
    }

    public void FinishSpawn(string run=null, float? lifetime=null)
    {
        // run default timeline
        run = run ?? Actor.DefaultRun;
        RunTimeline(run);
        // enforce lifetime
        lifetime = lifetime ?? Actor.Lifetime;
        if (lifetime != null && lifetime >= 0)
        {
            RunTimeline(new StageData.Actor.Timeline()
            {
                Name = "autogenerated_lifetime",
                Entries = new List<StageData.Actor.Timeline.Entry>()
                {
                    new StageData.Actor.Timeline.Entry()
                    {
                        Time = lifetime.Value,
                        Repeat = 1,
                        Event = new DestroyTimelineEvent() { Active = true }
                    }
                }
            });
        }
    }

    public bool IsRunningTimeline(string timeline)
    {
        foreach (RunningTimeline rt in RunningTimelines)
        {
            if (rt.Name == timeline)
            {
                return true;
            }
        }
        return false;
    }

    public void RunTimeline(string name)
    {
        if (name == null)
        {
            return;
        }
        var timelines = Actor.Timelines;
        if (!timelines.ContainsKey(name))
        {
            throw new StageDataException($"Attempting to run timeline named {name} on actor {ActorType} but no such timeline exists.");
        }
        // Debug.Log($"Running {name} with {timelines[name].Entries.Count} entries on actor {ActorType}");
        RunTimeline(timelines[name]);
    }
    void RunTimeline(StageData.Actor.Timeline timeline)
    {
        int timelineID = ++CurrentTimelineID;
        RunningTimelines.Add(new RunningTimeline()
        {
            ID = timelineID,
            Name = timeline.Name,
            Coroutine = StartCoroutine(RunTimelineCoroutine(timeline, timelineID))
        });
    }
    IEnumerator RunTimelineCoroutine(StageData.Actor.Timeline timeline, int timelineID)
    {
        float time = 0f;
        SortedList<RunningEvent, StageData.Actor.Timeline.IEvent> evSchedule = new SortedList<RunningEvent, StageData.Actor.Timeline.IEvent>();
        for (int i = 0; i < timeline.Entries.Count; i++)
        {
            var entry = timeline.Entries[i];
;           while (time < entry.Time)
            {
                FlushSchedule(evSchedule, time);
                yield return null;
                time += Time.deltaTime;
            }
            // Debug.Log($"Running entry at time {entry.Time} on actor {ActorType}");
            ScheduleEntry(evSchedule, entry);
        }
        // finish running scheduled events
        while (evSchedule.Count > 0)
        {
            FlushSchedule(evSchedule, time);
            yield return null;
            time += Time.deltaTime;
        }
        RunningTimelines.RemoveAll(x => x.ID == timelineID);
    }
    // call this when you add entries that run at the current time zero.
    public void FlushSchedule(SortedList<RunningEvent, StageData.Actor.Timeline.IEvent> evSchedule, float time)
    {
        while (evSchedule.Count > 0 && evSchedule.Keys.Min().Time <= time)
        {
            var entry = evSchedule.Keys.Min();
            evSchedule[entry].Start(this);
            evSchedule.Remove(entry);
        }
    }
    public void StopTimelinesWithName(string name)
    {
        foreach (RunningTimeline rt in RunningTimelines)
        {
            if (rt.Name == name)
            {
                StopCoroutine(rt.Coroutine);
            }
        }
        RunningTimelines.RemoveAll(x => x.Name == name);
    }
    public void StopAllTimelines()
    {
        foreach (RunningTimeline rt in RunningTimelines)
        {
            if (rt?.Coroutine != null)
            {
                StopCoroutine(rt.Coroutine);
            }
        }
        RunningTimelines.Clear();
    }

    void ScheduleEntry(SortedList<RunningEvent, StageData.Actor.Timeline.IEvent> evSchedule, StageData.Actor.Timeline.Entry entry)
    {
        int evID = ++EventID;
        int repeat = entry.Repeat ?? 1;
        float interval = entry.Interval ?? 0;
        float time = 0f;
        for (int i = 0; i < repeat; i++)
        {
            evSchedule.Add(new RunningEvent()
            {
                Time = entry.Time + (interval * i),
                EventID = evID,
                RepeatIndex = i
            }, entry.Event);
            time += interval;
        }
    }

    // enforce only one coroutine of this type allowed at once
    public void RunCoroutine(ref Coroutine crt, IEnumerator c)
    {
        if (crt != null)
        {
            StopCoroutine(crt);
        }
        if (c != null)
        {
            crt = StartCoroutine(c);
        }
    }
    public IEnumerator RotateCoroutine(float angle, float dur)
    {
        float time = 0f;
        float startAngle = transform.localEulerAngles.z;
        while (time < dur)
        {
            transform.localEulerAngles = new Vector3(0f, 0f, Mathf.Lerp(startAngle, angle, time / dur));
            time += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = new Vector3(0f, 0f, angle);
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
    public IEnumerator OrbitSpeedCoroutine(float orbitspeed, float dur)
    {
        float time = 0f;
        float startOrbitSpd = OrbitSpeed;
        while (time < dur)
        {
            OrbitSpeed = Mathf.Lerp(startOrbitSpd, orbitspeed, time / dur);
            time += Time.deltaTime;
            yield return null;
        }
        OrbitSpeed = orbitspeed;
    }
    public IEnumerator OrbitRadiusCoroutine(float radius, float dur)
    {
        float time = 0f;
        float startRadius = OrbitRadius ?? ((Vector2)transform.localPosition).magnitude;
        while (time < dur)
        {
            OrbitRadius = Mathf.Lerp(startRadius, radius, time / dur);
            time += Time.deltaTime;
            yield return null;
        }
        OrbitRadius = radius;
    }
    public IEnumerator OrbitTiltCoroutine(Vector3 tilt, float dur)
    {
        float time = 0f;
        Quaternion startTilt = OrbitTilt;
        Quaternion endTilt = Quaternion.Euler(tilt);
        while (time < dur)
        {
            OrbitTilt = Quaternion.Slerp(startTilt, endTilt, time / dur);
            time += Time.deltaTime;
            yield return null;
        }
        OrbitTilt = endTilt;
    }

    public IEnumerator OrbitCoroutine()
    {
        OrbitAngle = OrbitAngle ?? Mathf.Rad2Deg * Mathf.Atan2(transform.localPosition.y, transform.localPosition.x);
        OrbitRadius = OrbitRadius ?? ((Vector2)transform.localPosition).magnitude;
        while (true)
        {
            OrbitAngle += Time.deltaTime * OrbitSpeed * Mathf.Sign(Mirror.x) * Mathf.Sign(Mirror.y);
            Vector3 v = Quaternion.Euler(0f, 0f, OrbitAngle.Value) * Vector3.right * OrbitRadius.Value;
            v = OrbitTilt * v;
            transform.localPosition = new Vector3(v.x, v.y, (Actor.Depth ?? 0) + 2f * Mathf.Sign(-v.normalized.z - 0.1f));
            yield return null;
        }
    }

    // move in a direction for a duration, or forever
    const int ARCLEN_SEGMENTS = 7;
    const float SPLINE_TOOCLOSE = 0.01f;
    public IEnumerator MoveCoroutine(List<MoveTarget> spline, Vector2? finishdir=null, bool loop=false)
    {
        OrbitTilt = Quaternion.Euler(Vector3.zero);
        OrbitAngle = null;
        OrbitRadius = null;
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
                transform.localPosition = new Vector3(pos.x, pos.y, transform.localPosition.z);
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

    void DrawDebugDots(List<MoveTarget> spline)
    {
        // draw dots on control points
        DebugDotter.Clear();
        foreach (MoveTarget mt in spline)
        {
            DebugDotter.Dot(mt.Pre, Color.red);
            DebugDotter.Dot(mt.Pos, Color.magenta);
            DebugDotter.Dot(mt.Post, Color.blue);
        }
    }

    public void HandleDestroy(ActorDestroyReason reason)
    {
        switch (reason)
        {
            case ActorDestroyReason.Offscreen:
                RunTimeline(Actor?.OnDestroy?.Offscreen);
                break;
            case ActorDestroyReason.Impact:
                RunTimeline(Actor?.OnDestroy?.Impact);
                break;
            case ActorDestroyReason.Health:
                RunTimeline(Actor?.OnDestroy?.Impact);
                break;
            case ActorDestroyReason.Event:
                RunTimeline(Actor?.OnDestroy?.Event);
                break;
        }
    }

    // stage editor options on create
    void HandleStageEditorSpawn()
    {
        if (StageEditor.Instance != null)
        {
            if (StageEditor.Instance.DrawMovementPath && ActorType != "player")
            {
                gameObject.AddComponent<StageActorDrawMovementPath>();
            }
        }
    }
}
