using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {
    public float turnSpeed = 45;
    public float tiltSpeed = 45;
    public float rollSpeed = 45;
    public float rotationSpeed = 90;

    private float turnInput;
    private float tiltInput;
    private float rollInput;
    public float groundedDistance = 0.5f;

    private Vector3 currentEulerAngles;

    public BallMovement ballMove;

    void Start() {

    }

    void Update() {
        turnInput = Input.GetAxis("Horizontal");
        tiltInput = Input.GetAxis("Vertical");
        rollInput = Input.GetAxis("Roll");

        Vector3 rotationPerAxis = new Vector3(tiltInput, turnInput, rollInput) * Time.deltaTime * rotationSpeed;

        if (!ballMove.isGrounded) {
            transform.Rotate(transform.right, rotationPerAxis.x, Space.World);
            transform.Rotate(transform.forward, rotationPerAxis.z, Space.World);
        } else {
            if (Input.GetButtonDown("Jump")) {
                ballMove.Jump();
            }
        }
        transform.Rotate(transform.up, rotationPerAxis.y, Space.World);

    }
}
