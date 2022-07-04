using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayArea : MonoBehaviour
{
    public static PlayArea Instance;
    public BoxCollider2D Collider;

    void Awake()
    {
        Collider = GetComponent<BoxCollider2D>();
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public static bool WithinBounds(Vector3 pos, float border=10f)
    {
        Bounds bounds = Instance.Collider.bounds;
        bounds.Expand(border);
        return bounds.Contains(new Vector3(pos.x, pos.y, bounds.center.z));
    }
}
