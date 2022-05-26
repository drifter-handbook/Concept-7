using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputHandler input;
    [SerializeField] private MovementController movement;
    [SerializeField] private WeaponController weapon;
    void FixedUpdate()
    {
        if (input.move.pressed || input.move.released)
            movement.ChangeDir(input.dir);

        if (input.action1.down)
            weapon.Fire(WeaponType.PRIMARYRED);
        if (input.action2.down)
            weapon.Fire(WeaponType.PRIMARYYELLOW);
        if (input.action3.down)
            weapon.Fire(WeaponType.PRIMARYBLUE);
    }

    public void SetInput(InputHandler input) {
        this.input = input;
    }
}
