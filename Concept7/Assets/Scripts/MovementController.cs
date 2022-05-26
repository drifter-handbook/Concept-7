using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float upSpeed;
    [SerializeField] private float downSpeed;
    [SerializeField] private float leftSpeed;
    [SerializeField] private float rightSpeed;
    [Tooltip("multiplied by the current velocity each frame when smoothing especially large changes in speed direction")]
    [SerializeField] private float quickDecelMod;
    [SerializeField] private AnimationCurve vAccelCurve;
    [SerializeField] private AnimationCurve vDecelCurve;
    [SerializeField] private AnimationCurve hAccelCurve;
    [SerializeField] private AnimationCurve hDecelCurve;

    // mdir  -   preserves the direction to travel at any point, especially when 
    //           a key is released and a direction is needed for deceleration
    // dir   -   preserves the direction that was last inputed
    private Vector2 mdir, dir;
    private Vector2 speed;
    private float xTimestamp, yTimestamp;

    public void FixedUpdate() {

        // calculate whether to use acceleration or deceleration curves, and manages position on each curve
        Vector2 curve = Vector2.zero;
        curve.x = dir.x != 0 ? hAccelCurve.Evaluate(Time.time - xTimestamp) : hDecelCurve.Evaluate(Time.time - xTimestamp);
        curve.y = dir.y != 0 ? vAccelCurve.Evaluate(Time.time - yTimestamp) : vDecelCurve.Evaluate(Time.time - yTimestamp);

        Vector2 velocity = curve * speed * mdir;

        // adjusts potential velocity, applying quick deceleration modifiers for large changes in velocity
        if (dir.x != 0 && Mathf.Abs(velocity.x - rbody.velocity.x) > speed.x / 2 && Time.time - xTimestamp < 10 * Time.fixedDeltaTime) {
            velocity.x = rbody.velocity.x * quickDecelMod;
            xTimestamp = Time.time;
        }
        if (dir.y != 0 && Mathf.Abs(velocity.y - rbody.velocity.y) > speed.y / 2 && Time.time - yTimestamp < 10 * Time.fixedDeltaTime) {
            velocity.y = rbody.velocity.y * quickDecelMod;
            yTimestamp = Time.time;
        }    
        
        rbody.velocity = velocity;
    }

    public void ChangeDir(Vector2 ndir) {
        if (Mathf.Ceil(Mathf.Abs(ndir.x)) != Mathf.Ceil(Mathf.Abs(dir.x)) || Mathf.Sign(ndir.x) != Mathf.Sign(dir.x))
            xTimestamp = Time.time; 
            

        if (Mathf.Ceil(Mathf.Abs(ndir.y)) != Mathf.Ceil(Mathf.Abs(dir.y)) || Mathf.Sign(ndir.y) != Mathf.Sign(dir.y))
            yTimestamp = Time.time;

        // preserves movement direction once a key has been released
        mdir = new Vector2(ndir.x == 0 ? mdir.x : ndir.x, ndir.y == 0 ? mdir.y : ndir.y);

        // Set the master speed vector based on direction inputted
        speed = new Vector2(mdir.x > 0 ? rightSpeed : leftSpeed, mdir.y > 0 ? upSpeed : downSpeed);

        // adjust speed vector to use current speed for directions that have been released
        // avoids snapping to full speed before beginning deceleration
        if (ndir.x == 0)
            speed.x = Mathf.Abs(rbody.velocity.x);
        if (ndir.y == 0)
            speed.y = Mathf.Abs(rbody.velocity.y);

        dir = ndir;
    }
}
