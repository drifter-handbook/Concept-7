using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateLineRenderer : MonoBehaviour
{
    public Sprite SetSprite;
    LineRenderer lr;

    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SetSprite)
        {
            lr.material.mainTexture = SetSprite.texture;
        }
    }
}
