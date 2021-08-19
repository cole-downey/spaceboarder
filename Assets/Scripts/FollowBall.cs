using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowBall : MonoBehaviour {
    public GameObject ball;
    public BallMovement ballMove;
    private float ballRadius;
    public bool orientToGround = true;
    public float orientationSpeed = 180; // degrees per second 

    void Start() {
        ballMove = ball.GetComponent<BallMovement>();
        ballRadius = ball.transform.localScale.x / 2;
        transform.position = ball.transform.position;
    }

    void Update() {
        transform.position = ball.transform.position + ballMove.gravity.dir * (ballRadius);
        if(orientToGround && ballMove.isGrounded) {
            OrientToGround();
        }
    }

    public void OrientToGround() {
        Vector3 lookDir = Vector3.ProjectOnPlane(transform.forward, ballMove.surfaceNor).normalized;
        //Debug.DrawLine(transform.position, transform.position + lookDir, Color.green, 0.5f);
        var step = orientationSpeed * Time.deltaTime;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir, ballMove.surfaceNor), step);
    }
}
