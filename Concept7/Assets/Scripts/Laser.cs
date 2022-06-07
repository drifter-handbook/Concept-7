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

    LineRenderer lineRenderer;
    Vector2 prev;

    float UPDATE_DIST = 0.05f;
    float SEGMENT_DIST = 0.02f;
    int ARCLEN_SEGMENTS = 6;
    float MAX_LEN = 3f;

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        prev = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LateUpdate()
    {
        if (lineRenderer.positionCount == 0)
        {
            lineRenderer.positionCount = 1;
            lineRenderer.SetPositions(new Vector3[1] { transform.position });
            return;
        }
        // first two points to get initial direction
        if (lineRenderer.positionCount == 1)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new Vector3[2] { lineRenderer.GetPosition(0), transform.position });
            return;
        }
        Vector2 current = lineRenderer.GetPosition(lineRenderer.positionCount - 2);
        Vector2 diff = (Vector2)transform.position - current;
        // pull points from line renderer
        Vector3[] lrpoints = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(lrpoints);
        List<Vector3> points = new List<Vector3>(lrpoints);
        if (diff.magnitude >= UPDATE_DIST)
        {
            Vector2 dir = diff.normalized;
            int segments = Mathf.RoundToInt(diff.magnitude / SEGMENT_DIST);
            Vector2 cur = points[points.Count - 1];
            Vector2 curPost = cur + (cur - (Vector2)points[points.Count - 2]).normalized * 0.5f * diff.magnitude;
            Vector2 next = transform.position;
            Vector2 nextPre = next - ((Vector2)transform.position - prev).normalized * 0.5f * diff.magnitude;
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
            lineRenderer.SetPosition(lineRenderer.positionCount - 1, transform.position);
        }
        // filter points too close, and remove old segments
        float dist = 0f;
        for (int i = points.Count - 1; i >= 0; i--)
        {
            if (dist > MAX_LEN)
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
        // push to line renderer
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        // set head and tail
        if (lineRenderer.positionCount >= 2)
        {
            Head.SetActive(true);
            Head.transform.position = lineRenderer.GetPosition(0);
            Head.transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0)));
            Tail.SetActive(true);
            Tail.transform.position = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            Tail.transform.eulerAngles = new Vector3(0f, 0f, Vector2.SignedAngle(Vector2.right, lineRenderer.GetPosition(lineRenderer.positionCount - 1) - lineRenderer.GetPosition(lineRenderer.positionCount - 2)));
        }
        prev = transform.position;
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
