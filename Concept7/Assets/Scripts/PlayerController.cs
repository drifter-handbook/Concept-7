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

        //Toggles main weapon
        if(input.primary.pressed)movement.FireMainWeapon(true);
        else if(input.primary.released)movement.FireMainWeapon(false);
            
    }

    public void SetInput(InputHandler input) {
        this.input = input;
    }
}
