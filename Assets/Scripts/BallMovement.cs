using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour {
    public Transform character;

    public float gravStrength = 9.8f;
    private Vector3 gravDirection;
    public float forwardForce = 10f;
    public float carveForce = 10f;

    private Rigidbody rb;
    public bool isGrounded;
    private float groundedDistance = 0.05f;

    void Start() {
        rb = GetComponent<Rigidbody>();

        gravDirection = new Vector3(0.0f, -1.0f, 0.0f);

    }

    void FixedUpdate() {


        if (updateGrounded()) {
            rb.AddForce(Vector3.forward * forwardForce);
            // carving force
            rb.AddForce(character.right * -Vector3.Dot(character.right, rb.velocity) * carveForce);
        } else {
            rb.AddForce(gravDirection * gravStrength);
        }

    }

    bool updateGrounded() {
        int layerMask = 1 << 6;
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, gravDirection, out hit, groundedDistance + transform.localScale.x * 0.5f, layerMask);
        return isGrounded;
    }
}
