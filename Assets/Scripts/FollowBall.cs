using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBall : MonoBehaviour {
    public Transform ball;
    public Vector3 gravDir = Vector3.up;
    private float ballRadius;

    void Start() {
        ballRadius = ball.localScale.x / 2;
        transform.position = ball.position - gravDir * ballRadius;

    }

    void Update() {
        transform.position = ball.position - gravDir * ballRadius;
    }
}
