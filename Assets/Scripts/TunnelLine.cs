using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TunnelLine {
    public static TunnelLinePoint Evaluate(float t) {
        Vector3 pos = new Vector3(0f, 0f, t);
        Vector3 dir = new Vector3(0f, 0f, 1f).normalized;
        return new TunnelLinePoint(pos, dir);
    }
}

public struct TunnelLinePoint {
    Vector3 pos;
    Vector3 dir;
    public TunnelLinePoint(Vector3 _pos, Vector3 _dir) {
        pos = _pos;
        dir = _dir;
    }
}
