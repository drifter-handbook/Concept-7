using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveBullet : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rbody;
    [SerializeField] private Vector2 direction;
    [SerializeField] private float speed;
    // Start is called before the first frame update
    void Start()
    {
        rbody.velocity = speed * direction;
    }
}
