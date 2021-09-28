using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public struct Triangle
{
    public float surfaceArea;
    public Vector3 surfaceNormal;
    public Vector3 A, B, C; //vertices
    public Vector3 NA, NB, NC; //normals
    private Vector3 BA { get { return B - A; } }
    private Vector3 CA { get { return C - A; } }
    private Vector3 CB { get { return C - B; } }
    public Triangle(Vector3 A, Vector3 B, Vector3 C)
    {
        this.A = A;
        this.B = B;
        this.C = C;
        this.surfaceArea = (B - A).magnitude * (B - C).magnitude / 2;
        this.surfaceNormal = Vector3.Cross(B - A, C - A);
        this.NA = surfaceNormal;
        this.NB = surfaceNormal;
        this.NC = surfaceNormal;
    }
    public Triangle(Vector3 A, Vector3 B, Vector3 C, Vector3 NA, Vector3 NB, Vector3 NC)
    {
        this.A = A;
        this.B = B;
        this.C = C;
        this.surfaceArea = (B - A).magnitude * (B - C).magnitude / 2;
        this.surfaceNormal = Vector3.Cross(B - A, C - A);
        this.NA = NA;
        this.NB = NB;
        this.NC = NC;
    }

    public Vector3 InterpolatedNormal(Vector3 point)
    {
        var v0 = B - A;
        var v1 = C - A;
        var v2 = point - A;
        var d00 = Vector3.Dot(v0, v0);
        var d01 = Vector3.Dot(v0, v1);
        var d11 = Vector3.Dot(v1, v1);
        var d20 = Vector3.Dot(v2, v0);
        var d21 = Vector3.Dot(v2, v1);
        var denom = d00 * d11 - d01 * d01;
        var v = (d11 * d20 - d01 * d21) / denom;
        var w = (d00 * d21 - d01 * d20) / denom;
        var u = 1.0f - v - w;
        return (NA * v + NB * w + NC * u).normalized;
    }

    public Vector3 RandomPoint(System.Random random)
    {
        var t1 = (float)random.NextDouble();
        var t2 = (float)random.NextDouble();
        if (t1 + t2 >= 1)
        {
            t1 = 1 - t1;
            t2 = 1 - t2;
        }
        Vector3 point = BA * t1 + CA * t2;
        return point + A;
    }

    public Matrix4x4 RandomTRS(System.Random random)
    {
        var point = RandomPoint(random);
        var rotation = Quaternion.LookRotation(InterpolatedNormal(point));
        var scale = Vector3.one;
        return Matrix4x4.TRS(point, rotation, scale);
    }

    public int SweepTest(Vector3 point, Vector3 direction)
    {
        var E1 = B - A;
        var E2 = C - A;
        var N = Vector3.Cross(E1, E2);
        var det = -Vector3.Dot(direction, N);
        var invdet = 1f / det;
        var AO = point - A;
        var DAO = Vector3.Cross(AO, direction);
        var u = Vector3.Dot(E2, DAO) * invdet;
        var v = -Vector3.Dot(E1, DAO) * invdet;
        var t = Vector3.Dot(AO, N) * invdet;
        var intersects = (det >= float.MinValue && t >= 0 && u >= 0 && v >= 0 && (u + v) <= 1);
        return intersects ? Mathf.RoundToInt(Mathf.Sign(t)) : 0;
    }

    private float TetrahedonVolume(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
    {
        return Vector3.Dot(Vector3.Cross(B - A, C - A), D - A) / 6f;
    }
}
