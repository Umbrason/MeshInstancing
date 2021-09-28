using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class CustomShapeEmitter : SingleProfileEmitter
{

    private ConcurrentQueue<Batch> batchQueue = new ConcurrentQueue<Batch>();
    public List<CustomShape> shapes = new List<CustomShape>();
    public int meshCount;

    public override void GenerateBatches()
    {
        batches.Clear();
        if (mesh == null || material == null)
            return;
        var counter = this.meshCount;
        Random.InitState(seed);
        while (counter > 0)
        {
            var localToWorldMatrix = transform.localToWorldMatrix;
            int threadSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            int amount = Mathf.Min(1000, counter);
            Task t = new Task(() => batchQueue.Enqueue(GenerateRandomBatch(threadSeed, amount, localToWorldMatrix, shapes.Sum((x) => x.SurfaceArea))));
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
    private Batch GenerateRandomBatch(int seed, int batchSize, Matrix4x4 localToWorldMatrix, float totalSurfaceArea)
    {
        System.Random random = new System.Random(seed);
        var batch = new Batch(mesh, material);
        for (var i = 0; i < batchSize; i++)
        {
            var shape = PickRandomShape(random, totalSurfaceArea);
            var TRS = localToWorldMatrix * SampleRandomTRS(random, shape);
            batch.Add(TRS);
        }
        return batch;
    }

    private CustomShape PickRandomShape(System.Random random, float totalSurfaceArea)
    {
        if (shapes.Count == 0)
            return default;
        var shapeIndex = 0;
        var t = random.NextDouble() * totalSurfaceArea;
        while (t > shapes[shapeIndex].SurfaceArea)
        {
            t -= shapes[shapeIndex].SurfaceArea;
            shapeIndex++;
        }
        return shapes[shapeIndex];
    }

    private Matrix4x4 SampleRandomTRS(System.Random random, CustomShape shape)
    {
        var triangle = shape.RandomWeightedTriangle(random);
        var TRS = ApplyBasePosAndRandomRotScale(random, triangle.RandomTRS(random));
        return TRS;
    }

    [System.Serializable]
    public struct CustomShape
    {
        public List<Vector3> vertices;
        public List<int> triangleIndices;
        public float SurfaceArea { get { return Triangles.Sum((x) => x.surfaceArea); } }
        public List<Triangle> Triangles
        {
            get
            {
                var triangles = new List<Triangle>();
                for (int i = 0; i < triangleIndices.Count; i += 3)
                    triangles.Add(new Triangle(vertices[i + 0], vertices[i + 1], vertices[i + 2]));
                return triangles;
            }
        }

        public Triangle RandomWeightedTriangle(System.Random random)
        {
            var t = random.NextDouble() * SurfaceArea;
            var triangles = Triangles;
            var i = 0;
            while (t > triangles[i].surfaceArea)
            {
                t -= triangles[i].surfaceArea;
                i++;
            }
            return triangles[i];
        }
    }
}
