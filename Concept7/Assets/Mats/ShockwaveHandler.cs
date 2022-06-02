using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float duration;
    float time = 0;
    void Start() {
        sprite.material.SetFloat("_Duration", duration);    
    }
    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        sprite.material.SetFloat("_Phase", time);
        if (time > duration)
            Destroy(gameObject);
    }
}
