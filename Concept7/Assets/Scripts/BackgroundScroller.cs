using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public float scrollingSpeed = 2;
    private Renderer backgroundRenderer;

    void Start()
    {
        backgroundRenderer = GetComponent<Renderer>();
    }

    void FixedUpdate()
    {
        float offsetXval = Time.time * scrollingSpeed;
	    backgroundRenderer.sharedMaterial.mainTextureOffset = new Vector2 (offsetXval, 0);
    }
}
