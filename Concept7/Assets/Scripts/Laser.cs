using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// updates line renderer to follow current path
public class Laser : MonoBehaviour
{
    public GameObject Head;
    public GameObject Tail;

    List<Vector3> points = new List<Vector3>();

    LineRenderer lineRenderer;
    PolygonCollider2D polygonCollider;
    Vector2 prev;

    float UPDATE_DIST = 0.06f;
    float SEGMENT_DIST = 0.03f;
    int ARCLEN_SEGMENTS = 6;
    public float MAX_LEN = 15f;
    float POLY_SEGMENT_DIST = 0.2f;
    float POLY_SEGMENT_WIDTH = 0.2f;

    public float Lifetime;
    public float DecayTime;
    float LaserLength;

    // Start is called before the first frame update
    void Start()
    {
        LaserLength = MAX_LEN;
        lineRenderer = GetComponent<LineRenderer>();
        polygonCollider = GetComponent<PolygonCollider2D>();
        prev = transform.localPosition;
        StartCoroutine(LifetimeCoroutine());
    }
    IEnumerator LifetimeCoroutine()
    {
        float time = 0f;
        while (time < Lifetime - DecayTime)
        {
            yield return null;
            time += Time.deltaTime;
        }
        while (time < Lifetime)
        {
            float t = Mathf.InverseLerp(Lifetime - DecayTime, Lifetime, time);
            LaserLength = Mathf.Lerp(MAX_LEN, 0, t);
            yield return null;
            time += Time.deltaTime;
        }
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SyncToPoints()
    {
        // filter points too close, and remove old segments
        float dist = 0f;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (dist > LaserLength)
            {
                points.RemoveAt(i);
                continue;
            }
            if (i == 0 || i >= points.Count)
            {
                continue;
            }
            Vector2 dp = dp = points[i] - points[i - 1];
            if (dp.magnitude < SEGMENT_DIST * 0.2f)
            {
                points.RemoveAt(i - 1);
                // reprocess point
                i++;
                continue;
            }
            dist += dp.magnitude;
        }
        // set head and tail
        if (points.Count >= 2)
        {
            Head.SetActive(true);
            Head.transform.localPosition = points[0] - transform.localPosition;
            Head.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, points[1] - points[0]));
            Tail.SetActive(true);
            Tail.transform.localPosition = points[points.Count - 1] - transform.localPosition;
            Tail.transform.localEulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, points[points.Count - 1] - points[points.Count - 2]));
            // set polygon collider
            List<Vector2> polygonPointsTop = new List<Vector2>();
            List<Vector2> polygonPointsBottom = new List<Vector2>();
            float polydist = 0f;
            Vector2 curPos = points[0];
            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 polydiff = (Vector2)points[i] - (Vector2)points[i - 1];
                polydist += polydiff.magnitude;
                if (polydist > POLY_SEGMENT_DIST)
                {
                    // add polygon segment
                    Vector2 v = points[i];
                    Vector2 dir = (v - curPos).normalized;
                    polygonPointsTop.Add(v + new Vector2(-dir.y, dir.x) * 0.5f * POLY_SEGMENT_WIDTH - (Vector2)transform.localPosition);
                    polygonPointsBottom.Add(v + new Vector2(dir.y, -dir.x) * 0.5f * POLY_SEGMENT_WIDTH - (Vector2)transform.localPosition);
                    // reset
                    curPos = points[i];
                    polydist = 0f;
                }
            }
            polygonPointsBottom.Reverse();
            polygonPointsTop.AddRange(polygonPointsBottom);
            polygonCollider.points = polygonPointsTop.ToArray();
        }
        // push to line renderer
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.Select(x => x - transform.localPosition).ToArray());
    }
    void LateUpdate()
    {
        if (points.Count == 0)
        {
            points.Add(transform.localPosition);
            SyncToPoints();
            return;
        }
        // first two points to get initial direction
        if (points.Count == 1)
        {
            if (points[0] != transform.localPosition)
            {
                points.Add(transform.localPosition);
                SyncToPoints();
            }
            return;
        }
        Vector2 current = points[points.Count - 2];
        Vector2 diff = (Vector2)transform.localPosition - current;
        if (diff.magnitude >= UPDATE_DIST)
        {
            Vector2 dir = diff.normalized;
            int segments = Mathf.RoundToInt(diff.magnitude / SEGMENT_DIST);
            Vector2 cur = points[points.Count - 1];
            Vector2 curPost = cur + (cur - (Vector2)points[points.Count - 2]).normalized * 0.5f * diff.magnitude;
            Vector2 next = transform.localPosition;
            Vector2 nextPre = next - ((Vector2)transform.localPosition - prev).normalized * 0.5f * diff.magnitude;
            List<float> curveSpd = CurveSpeed(cur, curPost, nextPre, next);
            float totalDist = curveSpd.Sum();
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                // add points
                points.Add(CubicBezier(cur, curPost, nextPre, next, SpeedLerp(curveSpd, t * totalDist)));
            }
        }
        else
        {
            // move final point to current pos
            points[points.Count - 1] = transform.localPosition;
        }
        SyncToPoints();
        prev = transform.localPosition;
    }

    List<float> CurveSpeed(Vector2 cur, Vector2 curPost, Vector2 nextPre, Vector2 next)
    {
        // arc-length parameterizing
        List<Vector2> samples = new List<Vector2>();
        samples.Add(cur);
        for (int i = 1; i < ARCLEN_SEGMENTS; i++)
        {
            samples.Add(CubicBezier(cur, curPost, nextPre, next, (float)i / ARCLEN_SEGMENTS));
        }
        samples.Add(next);
        List<float> curveSpd = new List<float>();
        for (int j = 0; j < samples.Count - 1; j++)
        {
            curveSpd.Add((samples[j + 1] - samples[j]).magnitude);
        }
        return curveSpd;
    }
    Vector2 CubicBezier(Vector2 cur, Vector2 curPost, Vector2 nextPre, Vector2 next, float t)
    {
        Vector2 v = Vector2.Lerp(curPost, nextPre, t);
        Vector2 a = Vector2.Lerp(Vector2.Lerp(cur, curPost, t), v, t);
        Vector2 b = Vector2.Lerp(v, Vector2.Lerp(nextPre, next, t), t);
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
}
