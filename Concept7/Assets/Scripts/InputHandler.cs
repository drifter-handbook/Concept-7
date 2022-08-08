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
    private Button m_move;
    public Button move => m_move;
    private Button m_primary;
    public Button primary => m_primary;
    private Button m_secondary;
    public Button secondary => m_secondary;
    private Button m_action1;
    public Button action1 => m_action1;
    private Button m_action2;
    public Button action2 => m_action2;
    private Button m_action3;
    public Button action3 => m_action3;
    private Button m_action4;
    public Button action4 => m_action4;
    private Button m_action5;
    public Button action5=> m_action5;

    // Update is called once per frame
    void FixedUpdate()
    {
        m_move.Reset();
        m_primary.Reset();
        m_secondary.Reset();
        m_action1.Reset();
        m_action2.Reset();
        m_action3.Reset();
        m_action4.Reset();
        m_action5.Reset();
    }

    public void Move(InputAction.CallbackContext ctx) {
        dir = ctx.ReadValue<Vector2>();
        m_move.Set(ctx);
    }

    public void Primary(InputAction.CallbackContext ctx) {
        m_primary.Set(ctx);
    }

    public void Secondary(InputAction.CallbackContext ctx) {
        m_secondary.Set(ctx);
    }

    public void Action1(InputAction.CallbackContext ctx) {
        m_action1.Set(ctx);
    }

    public void Action2(InputAction.CallbackContext ctx) {
        m_action2.Set(ctx);
    }

    public void Action3(InputAction.CallbackContext ctx) {
        m_action3.Set(ctx);
    }

    public void Action4(InputAction.CallbackContext ctx) {
        m_action4.Set(ctx);
    }
}


public struct Button {
    public bool down, released, pressed;

    public void Reset() {
        released = false;
        pressed = false;
    }

    public void Set(InputAction.CallbackContext ctx) {
        down = ctx.canceled == false;

        released = !down;
        pressed = down;
    }
}