using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private float upSpeed;
    [SerializeField] private float downSpeed;
    [SerializeField] private float leftSpeed;
    [SerializeField] private float rightSpeed;
    [SerializeField] private AnimationCurve vAccelCurve;
    [SerializeField] private AnimationCurve vDecelCurve;
    [SerializeField] private AnimationCurve hAccelCurve;
    [SerializeField] private AnimationCurve hDecelCurve;
    private Vector2 mdir, dir;
    private Vector2 speed;
    private float xTimestamp, yTimestamp;

    public void FixedUpdate() {
        Vector2 curve = Vector2.zero;
        curve.x = dir.x != 0 ? hAccelCurve.Evaluate(Time.time - xTimestamp) : hDecelCurve.Evaluate(Time.time - xTimestamp);
        curve.y = dir.y != 0 ? vAccelCurve.Evaluate(Time.time - yTimestamp) : vDecelCurve.Evaluate(Time.time - yTimestamp);

        rbody.velocity = curve * speed * mdir;
    }

    public void ChangeDir(Vector2 ndir) {
        if (Mathf.Ceil(Mathf.Abs(ndir.x)) != Mathf.Ceil(Mathf.Abs(dir.x)))
            xTimestamp = Time.time;

        if (Mathf.Ceil(Mathf.Abs(ndir.y)) != Mathf.Ceil(Mathf.Abs(dir.y)))
            yTimestamp = Time.time;

        dir = ndir;
        mdir = new Vector2(ndir.x == 0 ? mdir.x : ndir.x, ndir.y == 0 ? mdir.y : ndir.y);

        speed = new Vector2(mdir.x > 0 ? rightSpeed : leftSpeed, mdir.y > 0 ? upSpeed : downSpeed);
    }
}
