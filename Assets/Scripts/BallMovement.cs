using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour {
    public GameObject character;
    public CharacterMovement characterMovement;

    public float gravStrength = 9.8f;
    private Vector3 gravDirection;
    public float forwardForce = 10f;
    public float carveForce = 10f;
    public float jumpForce = 10f;

    private Rigidbody rb;
    public bool isGrounded;
    private float groundedDistance = 0.2f;
    private int layerMask;
    public Vector3 surfaceNor;

    void Start() {
        rb = GetComponent<Rigidbody>();
        characterMovement = character.GetComponent<CharacterMovement>();
        gravDirection = new Vector3(0.0f, -1.0f, 0.0f);
        layerMask = 1 << 6;
    }

    void FixedUpdate() {
        if (UpdateGrounded()) {
            //characterMovement.OrientToGround(surfaceNor);
            
            rb.AddForce(gravDirection * gravStrength, ForceMode.Acceleration);
            rb.AddForce(Vector3.forward * forwardForce);
            // carving force
            rb.AddForce(character.transform.right * -Vector3.Dot(character.transform.right, rb.velocity) * carveForce);
        } else {
            rb.AddForce(gravDirection * gravStrength);
        }

    }

    bool UpdateGrounded() {
        RaycastHit hit;
        isGrounded = Physics.Raycast(transform.position, gravDirection, out hit, groundedDistance + transform.localScale.x * 0.5f, layerMask);
        surfaceNor = hit.normal.normalized;
        return isGrounded;
    }

    public void Jump() {
        rb.AddForce(surfaceNor * jumpForce, ForceMode.Impulse);
    }
}
