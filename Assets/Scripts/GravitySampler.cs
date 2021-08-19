using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GravitySampler {
    private static Vector3 tunnelCenter = Vector3.zero;
    private static float gravStrength = 9.8f;

    public static GravData Sample(Vector3 point) {
        var newDir = point - new Vector3(0f, 0f, point.z);
        newDir.Normalize();
        return new GravData(newDir, gravStrength);
    }
    public static void SetGravStrength(float newStrength) {
        gravStrength = newStrength;
    }
}

public class GravData {
    public readonly Vector3 dir;
    public readonly float strength;
    public GravData(Vector3 dir, float strength) {
        this.dir = dir;
        this.strength = strength;
    }
}
