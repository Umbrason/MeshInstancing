using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshEmitter : SingleProfileEmitter
{
    public Mesh emitterMesh;
    public int meshCount;
    public MeshEmissionType emissionType;

    public override void GenerateBatches()
    {
        batches.Clear();
        if (!emitterMesh)
            return;

        Random.InitState(seed);
        int counter = meshCount;
        var triangles = new List<Triangle>(emitterMesh.triangles.Length / 3);
        var totalSurfaceArea = 0f;
        for (var i = 0; i < emitterMesh.triangles.Length; i += 3)
        {
            var A = emitterMesh.vertices[emitterMesh.triangles[i + 0]];
            var B = emitterMesh.vertices[emitterMesh.triangles[i + 1]];
            var C = emitterMesh.vertices[emitterMesh.triangles[i + 2]];
            var NA = emitterMesh.normals[emitterMesh.triangles[i + 0]];
            var NB = emitterMesh.normals[emitterMesh.triangles[i + 1]];
            var NC = emitterMesh.normals[emitterMesh.triangles[i + 2]];
            var triangle = new Triangle(A, B, C, NA, NB, NC);
            totalSurfaceArea += triangle.surfaceArea;
            triangles.Add(triangle);
        }
        while (counter > 0)
        {
            var batch = new Batch(mesh, material);
            for (var i = 0; i < Mathf.Min(counter, 1000); i++)
            {
                var TRS = transform.localToWorldMatrix * SampleTRS(Random.state, triangles, totalSurfaceArea);
                batch.Add(TRS);
            }
            batches.Add(batch);
            counter -= 1000;
        }
    }

    private Matrix4x4 SampleTRS(Random.State state, List<Triangle> triangles, float totalSurfaceArea)
    {
        Random.state = state;
        switch (emissionType)
        {
            case MeshEmissionType.Volume:
                return SampleVolumeTRS();
            case MeshEmissionType.Surface:
            default:
                return SampleSurfaceTRS(triangles, totalSurfaceArea);
        }
    }

    private Matrix4x4 SampleSurfaceTRS(List<Triangle> triangles, float totalSurfaceArea)
    {
        if (triangles == null || triangles.Count == 0)
            return default;
        float t = Random.value * totalSurfaceArea;
        int i = 0;
        while (t > triangles[i].surfaceArea)
        {
            t -= triangles[i].surfaceArea;
            i++;
        }
        var triangle = triangles[i];
        var position = triangle.RandomPoint();
        var scale = baseScale + Vector3.Scale(RandomVector(), randomScaleRange);
        var rotation = Quaternion.LookRotation(triangle.normal) * Quaternion.Euler(baseRotation + Vector3.Scale(RandomVector(), randomRotationRange));
        return Matrix4x4.TRS(position, rotation, scale);
    }

    private Vector3 RandomVector()
    {
        return new Vector3(Random.value, Random.value, Random.value);
    }

    private Matrix4x4 SampleVolumeTRS()
    {
        throw new System.NotImplementedException();
    }

    private struct EmitterSurfaceData
    {
        List<Triangle> triangles;
    }

    private struct Triangle
    {
        public float surfaceArea;
        public Vector3 normal;
        public Vector3 A, B, C;
        public Triangle(Vector3 A, Vector3 B, Vector3 C)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.surfaceArea = (B - A).magnitude * (B - C).magnitude / 2;
            this.normal = Vector3.Cross(B - A, C - A);
        }
        public Triangle(Vector3 A, Vector3 B, Vector3 C, Vector3 NA, Vector3 NB, Vector3 NC)
        {
            this.A = A;
            this.B = B;
            this.C = C;
            this.surfaceArea = (B - A).magnitude * (B - C).magnitude / 2;
            this.normal = (NA + NB + NC).normalized;
        }

        public Vector3 RandomPoint()
        {
            Vector3 BA = B - A;
            Vector3 CA = C - A;
            var t1 = Random.value;
            var t2 = Random.value;
            if (t1 + t2 >= 1)
            {
                t1 = 1 - t1;
                t2 = 1 - t2;
            }
            Vector3 point = BA * t1 + CA * t2;
            return point + A;
        }
    }

    [System.Serializable]
    public enum MeshEmissionType
    {
        Volume, Surface
    }
}
