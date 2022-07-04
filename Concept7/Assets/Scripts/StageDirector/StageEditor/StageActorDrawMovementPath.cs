using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageActorDrawMovementPath : MonoBehaviour
{
    LineRenderer lr;
    Vector2 last;
    const float UPDATE_DIST = 0.1f;
    public Material MovementPathMaterial;

    // Start is called before the first frame update
    void Start()
    {
        lr = gameObject.AddComponent<LineRenderer>();
        lr.material = MovementPathMaterial;
        lr.widthMultiplier = 0.1f;
        lr.startColor = Color.blue;
        lr.endColor = Color.blue;
        lr.positionCount = 0;
        last = transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (((Vector2)transform.parent.position - last).magnitude > UPDATE_DIST)
        {
            Vector3[] points = new Vector3[lr.positionCount + 1];
            lr.GetPositions(points);
            points[points.Length - 1] = (Vector2)transform.parent.position;
            lr.positionCount = points.Length;
            lr.SetPositions(points);
        }
    }
}
