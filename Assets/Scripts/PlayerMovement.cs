using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour {
    public float turnSpeed = 1;
    public float tiltSpeed = 1;
    public float rollSpeed = 1;

    public Transform board;
    public Rigidbody fWheel;
    public Rigidbody bWheel;

    private Rigidbody rb;
    private float turnInput;
    private float tiltInput;
    private float rollInput;
    private Vector3 gravDirection;
    public float groundedDistance = 0.5f;

    void Start() {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 5;
        //rb.constraints = RigidbodyConstraints.FreezeRotationY; // so terrain doesn't affect turn

        gravDirection = new Vector3(0.0f, -1.0f, 0.0f);
    }

    void Update() {
        turnInput = Input.GetAxis("Horizontal");
        tiltInput = Input.GetAxis("Vertical");
        rollInput = Input.GetAxis("Roll");
    }
    void FixedUpdate() {
        Vector3 gravity = new Vector3(0.0f, -2.0f, 1.0f);
        float gravStrength = 9.8f * 2;
        fWheel.AddForce(gravity * gravStrength, ForceMode.Acceleration);
        bWheel.AddForce(gravity * gravStrength, ForceMode.Acceleration);

        //int layerMask = 1 << 6;
        //RaycastHit hit;
        
        /*
        if (Physics.Raycast(board.position, gravDirection, out hit, groundedDistance, layerMask)) {
            Vector3 surfaceNor = hit.normal.normalized;
            Vector3 lookDir = new Vector3(Mathf.Sin(turnInput * Mathf.PI / 2.0f), 0.0f, Mathf.Cos(turnInput * Mathf.PI / 2.0f)).normalized;
            Quaternion groundedRot = Quaternion.LookRotation(lookDir, surfaceNor);
            rb.MoveRotation(groundedRot);
        } else {
            Vector3 lookDir = new Vector3(Mathf.Sin(turnInput * Mathf.PI / 2.0f), 0.0f, Mathf.Cos(turnInput * Mathf.PI / 2.0f)).normalized;
            Quaternion airRot = Quaternion.LookRotation(lookDir, -gravDirection);
            rb.MoveRotation(airRot);
        }
        */
        

        //rb.angularVelocity = new Vector3(tiltInput * tiltSpeed, turnInput * turnSpeed, rollInput * rollSpeed);
        //rb.AddRelativeTorque(new Vector3(-tiltSpeed * tiltInput, turnInput * turnSpeed, -rollSpeed * rollInput), ForceMode.VelocityChange);
        //Quaternion rot = new Quaternion();
        //Vector3 eulRot = new Vector3(rb.rotation.eulerAngles.x, turnInput * 90.0f, rb.rotation.eulerAngles.z);
        //rot.eulerAngles = eulRot;
        //rb.MoveRotation(rot);
        //rb.AddRelativeTorque(Vector3.up * turnSpeed * turnInput, ForceMode.VelocityChange);

    }
}
