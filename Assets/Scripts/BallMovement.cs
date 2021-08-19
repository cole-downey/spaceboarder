using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallMovement : MonoBehaviour {
    public GameObject character;
    public CharacterMovement characterMovement;
    public Transform[] groundSamplers;

    public float gravStrength = 9.8f;
    public GravData gravity;
    public float forwardForce = 10f;
    public float carveForce = 10f;
    public float jumpForce = 10f;

    private Rigidbody rb;
    public bool isGrounded;
    public float groundedDistance = 0.12f;
    private int layerMask;
    public Vector3 surfaceNor;
    private Vector3 lastCollisionDir;

    void Start() {
        rb = GetComponent<Rigidbody>();
        characterMovement = character.GetComponent<CharacterMovement>();
        layerMask = 1 << 6;
        groundSamplers = new Transform[4];
        groundSamplers[0] = character.transform.Find("GroundSamplerFR");
        groundSamplers[1] = character.transform.Find("GroundSamplerFL");
        groundSamplers[2] = character.transform.Find("GroundSamplerBR");
        groundSamplers[3] = character.transform.Find("GroundSamplerBL");

        GravitySampler.SetGravStrength(gravStrength);
    }

    void FixedUpdate() {
        gravity = GravitySampler.Sample(transform.position);
        Debug.DrawLine(transform.position, gravity.dir + transform.position, Color.magenta);
        rb.AddForce(gravity.dir * gravity.strength, ForceMode.Acceleration);
        if (UpdateGrounded()) {
            //characterMovement.OrientToGround(surfaceNor);
            rb.AddForce(Vector3.forward * forwardForce);
            // carving force
            rb.AddForce(character.transform.right * -Vector3.Dot(character.transform.right, rb.velocity) * carveForce);
        }

    }

    bool UpdateGrounded() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, gravity.dir, out hit, groundedDistance + transform.localScale.x * 0.5f, layerMask)) {
            isGrounded = true;
            surfaceNor = hit.normal;
        } else if (Physics.Raycast(transform.position, lastCollisionDir, out hit, groundedDistance + transform.localScale.x * 0.5f, layerMask)) {
            // if gravity raycast doesn't hit, try casting towards last collider hit
            isGrounded = true;
            surfaceNor = hit.normal;
        } else {
            isGrounded = false;
        }
        return isGrounded;
        /*
        foreach (var sampler in groundSamplers) {
            if (Physics.Raycast(sampler.position, sampler.forward, out hit, groundedDistance, layerMask)) {
                tempNor += hit.normal;
            }
            //Debug.DrawLine(sampler.position, sampler.position + sampler.forward * groundedDistance, Color.green);
        }
        */
    }

    public void Jump() {
        rb.AddForce(surfaceNor * jumpForce, ForceMode.Impulse);
    }

    public void OnCollisionStay(Collision collInfo) {
        foreach (var contact in collInfo.contacts) {
            if (contact.otherCollider.tag == "Terrain") {
                lastCollisionDir = (contact.point - transform.position).normalized;
            }
        }
    }

}
