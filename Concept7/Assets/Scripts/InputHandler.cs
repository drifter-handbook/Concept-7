using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    public Vector2 dir {
        get;
        private set;
    }
    public Button move { get; private set; } = new Button();
    public Button primary { get; private set; } = new Button();
    public Button secondary { get; private set; } = new Button();
    public Button action1 { get; private set; } = new Button();
    public Button action2 { get; private set; } = new Button();
    public Button action3 { get; private set; } = new Button();
    public Button action4 { get; private set; } = new Button();
    public Button pause { get; private set; } = new Button();

    // Update is called once per frame
    void FixedUpdate()
    {
        move.Reset();
        primary.Reset();
        secondary.Reset();
        action1.Reset();
        action2.Reset();
        action3.Reset();
        action4.Reset();
    }

    void Update()
    {
        
    }

    public void ResetPause()
    {
        pause.Reset();
    }

    public void Move(InputAction.CallbackContext ctx) {
        dir = ctx.ReadValue<Vector2>();
        move.Set(ctx);
    }

    public void Primary(InputAction.CallbackContext ctx) {
        primary.Set(ctx);
    }

    public void Secondary(InputAction.CallbackContext ctx) {
        secondary.Set(ctx);
    }

    public void Action1(InputAction.CallbackContext ctx) {
        action1.Set(ctx);
    }

    public void Action2(InputAction.CallbackContext ctx) {
        action2.Set(ctx);
    }

    public void Action3(InputAction.CallbackContext ctx) {
        action3.Set(ctx);
    }

    public void Action4(InputAction.CallbackContext ctx) {
        action4.Set(ctx);
    }

    public void Pause(InputAction.CallbackContext ctx) {
        pause.Set(ctx);
    }
}


public class Button {
    public bool down, released, pressed;

    public void Reset() {
        released = false;
        pressed = false;
    }

    public void Set(InputAction.CallbackContext ctx) {
        down = ctx.canceled == false;

        released = !down || released;
        pressed = down || pressed;
    }
}