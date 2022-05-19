using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputHandler input;
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float speed;
    void FixedUpdate()
    {
        rbody.velocity = speed * input.dir;
    }

    public void SetInput(InputHandler input) {
        this.input = input;
    }
}
