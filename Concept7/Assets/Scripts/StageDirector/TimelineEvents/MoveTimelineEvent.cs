using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

public class MoveTimelineEvent : StageData.Actor.Timeline.IEvent
{
    public string Action => "move";
    
    public float? Speed;
    public bool Instant;
    public bool Loop;
    public List<Destination> Dest;

    public class ControlPt
    {
        public float? X;
        public float? Y;
        public float? Dir;
        public float? Dist;
        public string Rel;
    }

    public class Destination
    {
        public float? X;
        public float? Y;
        public float? Dir;
        public float? Dist;
        public string Rel;
        public ControlPt Pre;
        public ControlPt Post;

        public bool IsInfinite => X == null && Y == null && Dist == null;

        public StageActor.MoveTarget FindControlPoints(Vector2 prev, Vector2 cur, Vector2 next, Vector2 dir)
        {
            if (Pre != null && Post == null)
            {
                Vector2 v = TimelineEventUtils.FindDestPosition(Pre.X, Pre.Y, Pre.Dir, Pre.Dist, Pre.Rel ?? Rel, cur, dir);
                return new StageActor.MoveTarget()
                {
                    Pos = cur,
                    Pre = v,
                    Post = (cur - v) + cur
                };
            }
            if (Pre == null && Post != null)
            {
                Vector2 v = TimelineEventUtils.FindDestPosition(Post.X, Post.Y, Post.Dir, Post.Dist, Post.Rel ?? Rel, cur, dir);
                return new StageActor.MoveTarget()
                {
                    Pos = cur,
                    Pre = cur - (v - cur),
                    Post = v
                };
            }
            if (Pre != null && Post != null)
            {
                return new StageActor.MoveTarget()
                {
                    Pos = cur,
                    Pre = TimelineEventUtils.FindDestPosition(Pre.X, Pre.Y, Pre.Dir, Pre.Dist, Pre.Rel ?? Rel, cur, dir),
                    Post = TimelineEventUtils.FindDestPosition(Post.X, Post.Y, Post.Dir, Post.Dist, Post.Rel ?? Rel, cur, dir)
                };
            }
            return new StageActor.MoveTarget()
            {
                Pos = cur,
                Pre = Vector2.Lerp(prev, cur, 0.5f),
                Post = Vector2.Lerp(cur, next, 0.5f)
            };
        }
    }

    public StageData.Actor.Timeline.IEvent CloneFrom(string yaml, IDeserializer deserializer)
    {
        return deserializer.Deserialize<MoveTimelineEvent>(yaml);
    }

    Vector2 Mirror(Vector2 pt, Vector2 origin, Vector2 mirror, Vector2 dir)
    {
        Vector2 v = pt - origin;
        v = new Vector2(v.x * Mathf.Sign(mirror.x), v.y * Mathf.Sign(mirror.y));
        return origin + v;
    }

    public void Start(MonoBehaviour runner)
    {
        if (Dest.Count == 0)
        {
            return;
        }
        List<Destination> dests = new List<Destination>(Dest);
        StageActor actor = runner.GetComponent<StageActor>();
        // filter out 'move infinitely' command
        Destination infinite = null;
        for (int i = 0; i < dests.Count; i++)
        {
            if (dests[i].IsInfinite)
            {
                infinite = dests[i];
            }
            // if we've already found an infinite dest, delete the rest
            if (infinite != null)
            {
                dests.RemoveAt(i);
                i--;
            }
        }
        // calculate positions
        List<StageActor.MoveTarget> spline = new List<StageActor.MoveTarget>();
        if (dests.Count > 0)
        {
            List<Vector2> pos = new List<Vector2>();
            Vector2 current = runner.transform.localPosition;
            pos.Add(current);
            for (int i = 0; i < dests.Count; i++)
            {
                Destination d = dests[i];
                current = TimelineEventUtils.FindDestPosition(d.X, d.Y, d.Dir, d.Dist, d.Rel, runner.transform.localPosition, actor.Direction);
                pos.Add(current);
            }
            // calculate Move Targets
            Destination startDest = new Destination { X = actor.transform.localPosition.x, Y = actor.transform.localPosition.y };
            spline.Add(startDest.FindControlPoints(pos[0], pos[0], pos[1], actor.Direction));
            for (int i = 1; i < pos.Count - 1; i++)
            {
                spline.Add(dests[i - 1].FindControlPoints(pos[i - 1], pos[i], pos[i + 1], actor.Direction));
            }
            spline.Add(dests[dests.Count - 1].FindControlPoints(pos[pos.Count - 2], pos[pos.Count - 1], pos[pos.Count - 1], actor.Direction));
            // mirror spline if necessary
            Vector2 startPos = spline[0].Pos;
            for (int i = 0; i < spline.Count - 1; i++)
            {
                StageActor.MoveTarget pt = spline[i + 1];
                if (dests[i].Rel != "abs")
                {
                    pt.Pos = Mirror(pt.Pos, runner.transform.localPosition, actor.Mirror, dests[i].Rel == "dir" ? actor.Direction : Vector2.right);
                    pt.Pre = Mirror(pt.Pre, runner.transform.localPosition, actor.Mirror, dests[i].Rel == "dir" ? actor.Direction : Vector2.right);
                    pt.Post = Mirror(pt.Post, runner.transform.localPosition, actor.Mirror, dests[i].Rel == "dir" ? actor.Direction : Vector2.right);
                }
            }
        }
        // set initial speed
        if (Speed != null)
        {
            actor.Speed = Speed.Value;
        }
        // if moving infinitely, set finishdir
        Vector2? finishdir = null;
        if (infinite != null)
        {
            finishdir = Quaternion.Euler(0f, 0f, infinite.Dir ?? 0) * Vector2.right;
        }
        // if instant, move to final point
        if (Instant && spline.Count > 0)
        {
            actor.transform.localPosition = spline[spline.Count - 1].Pos;
            if (finishdir != null)
            {
                actor.Direction = finishdir.Value;
            }
        }
        // if normal movement, kick off coroutine
        else
        {
            actor.RunCoroutine(ref actor.movementCoroutine, actor.MoveCoroutine(spline, finishdir, Loop));
        }
    }
}