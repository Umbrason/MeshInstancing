using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using System.Threading.Tasks;
using System;

public class MeshEmitter : SingleProfileEmitter
{
    public Mesh emitterMesh;
    public int meshCount;
    [Serializable]
    public enum MeshEmissionType
    {
        Volume, Surface
    }
    public MeshEmissionType emissionType;

    private ConcurrentQueue<Batch> batchQueue = new ConcurrentQueue<Batch>();
    private Queue<Task> batchGenerationTasks = new Queue<Task>();

    public override void GenerateBatches()
    {
        batches.Clear();
        if (!emitterMesh)
            return;
        while (batchGenerationTasks.Count > 0)
            batchGenerationTasks.Dequeue().Dispose();
        batchQueue = new ConcurrentQueue<Batch>();
        UnityEngine.Random.InitState(seed);
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
            var localToWorldMatrix = transform.localToWorldMatrix;
            int threadSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            int amount = Mathf.Min(1000, counter);            
            Task t = new Task(() => batchQueue.Enqueue(GenerateRandomBatch(threadSeed, amount, localToWorldMatrix, triangles, totalSurfaceArea)));
            t.Start();
            counter -= 1000;
        }
    }
#if UNITY_EDITOR
    public new void Update()
    {
        if (batchQueue.Count > 0 && batchQueue.TryDequeue(out Batch batch))
            batches.Add(batch);
        base.Update();
    }
#endif

    private Batch GenerateRandomBatch(int seed, int size, Matrix4x4 localToWorldMatrix, List<Triangle> triangles, float totalSurfaceArea)
    {
        System.Random random = new System.Random(seed);
        var batch = new Batch(mesh, material);
        for (var i = 0; i < size; i++)
        {
            var TRS = localToWorldMatrix * SampleRandomTRS(random, triangles, totalSurfaceArea);
            batch.Add(TRS);
        }
        return batch;
    }

    private Matrix4x4 SampleRandomTRS(System.Random random, List<Triangle> triangles, float totalSurfaceArea)
    {
        switch (emissionType)
        {
            case MeshEmissionType.Volume:
                return SampleRandomVolumeTRS(random, triangles);
            case MeshEmissionType.Surface:
            default:
                return SampleRandomSurfaceTRS(random, triangles, totalSurfaceArea);
        }
    }

    private Matrix4x4 SampleRandomSurfaceTRS(System.Random random, List<Triangle> triangles, float totalSurfaceArea)
    {
        if (triangles == null || triangles.Count == 0)
            return default;
        float t = ((float)random.NextDouble()) * totalSurfaceArea;
        int i = 0;
        while (t > triangles[i].surfaceArea)
        {
            t -= triangles[i].surfaceArea;
            i++;
        }

        var triangle = triangles[i];
        var TRS = triangle.RandomTRS(random);
        return ApplyBasePosAndRandomRotScale(random, TRS);
    }

    private Matrix4x4 SampleRandomVolumeTRS(System.Random random, List<Triangle> triangles)
    {
        var min = emitterMesh.bounds.min;
        var delta = emitterMesh.bounds.max - emitterMesh.bounds.min;
        var p = Vector3.zero;
        int attempts = 0;
        do
        {
            p = Vector3.Scale(Utility.RandomVector(random), delta) + min;
            attempts++;
        }
        while (Utility.MeshSweepTest(triangles, p, Vector3.up) && attempts < 100);
        var scale = baseScale + Vector3.Scale(Utility.RandomVector(random), randomScaleRange);
        var normal = Vector3.zero;
        foreach (var triangle in triangles)
            normal += triangle.surfaceNormal / ((triangle.A + triangle.B + triangle.C) / 3 - p).sqrMagnitude;
        normal.Normalize();
        var rotation = Quaternion.LookRotation(normal) * Quaternion.Euler(baseRotation) * Quaternion.Euler(Vector3.Scale(Utility.RandomVector(random) * 2 - Vector3.one, randomRotationRange));
        return Matrix4x4.TRS(p, rotation, scale);
    }



}
