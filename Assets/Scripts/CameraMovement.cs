using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform player;
    private Transform cam;
    public Material terrainMat;
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        cam.position = player.position + new Vector3(0.0f, 1.0f, -10.0f);
        terrainMat.SetVector("CamPosition", cam.position);
    }
}
