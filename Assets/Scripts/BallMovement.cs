using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour {
    public GameObject character;
    public CharacterMovement characterMovement;
    public Transform[] groundSamplers;

    public float gravStrength = 9.8f;
    private Vector3 gravDirection;
    public float forwardForce = 10f;
    public float carveForce = 10f;
    public float jumpForce = 10f;

    private Rigidbody rb;
    public bool isGrounded;
    public float groundedDistance = 0.12f;
    private int layerMask;
    public Vector3 surfaceNor;

    void Start() {
        rb = GetComponent<Rigidbody>();
        characterMovement = character.GetComponent<CharacterMovement>();
        gravDirection = new Vector3(0.0f, -1.0f, 0.0f);
        layerMask = 1 << 6;
        groundSamplers = new Transform[4];
        groundSamplers[0] = character.transform.Find("GroundSamplerFR");
        groundSamplers[1] = character.transform.Find("GroundSamplerFL");
        groundSamplers[2] = character.transform.Find("GroundSamplerBR");
        groundSamplers[3] = character.transform.Find("GroundSamplerBL");
    }

    void FixedUpdate() {
        rb.AddForce(gravDirection * gravStrength, ForceMode.Acceleration);
        if (UpdateGrounded()) {
            //characterMovement.OrientToGround(surfaceNor);
            rb.AddForce(Vector3.forward * forwardForce);
            // carving force
            rb.AddForce(character.transform.right * -Vector3.Dot(character.transform.right, rb.velocity) * carveForce);
        } else {
        }

    }

    bool UpdateGrounded() {
        RaycastHit hit;
        bool tempGrounded = false;
        Vector3 tempNor = Vector3.zero;
        foreach (var sampler in groundSamplers) {
            bool thisGrounded = Physics.Raycast(sampler.position, sampler.forward, out hit, groundedDistance, layerMask);
            if(thisGrounded) {
                tempGrounded = true;
                tempNor += hit.normal;
            }
        }
        //isGrounded = Physics.Raycast(transform.position, gravDirection, out hit, groundedDistance + transform.localScale.x * 0.5f, layerMask);
        isGrounded = tempGrounded;
        surfaceNor = tempNor.normalized;
        return isGrounded;
    }

    public void Jump() {
        rb.AddForce(surfaceNor * jumpForce, ForceMode.Impulse);
    }
}
