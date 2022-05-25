using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputHandler input;
    [SerializeField] private MovementController movement;
    void FixedUpdate()
    {
        if (input.move.pressed || input.move.released)
            movement.ChangeDir(input.dir);
    }

    public void SetInput(InputHandler input) {
        this.input = input;
    }
}
