using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour {
    public Transform player;
    public Material terrainMat;
    public float distFromPlayer = 10f;
    [Range (0, 90)]
    public float angle = 45f;

    void LateUpdate() {
        var up = -GravitySampler.Sample(player.position).dir;
        transform.position = player.position + new Vector3(up.x * Mathf.Sin(angle * Mathf.Deg2Rad), up.y * Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad)).normalized * distFromPlayer;
        transform.LookAt(player, up);
        terrainMat.SetVector("CamPosition", transform.position);
    }
}
