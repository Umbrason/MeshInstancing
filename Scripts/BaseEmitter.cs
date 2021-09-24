using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEmitter : MonoBehaviour
{
    public int seed;
    public List<Batch> batches;
    public abstract void GenerateBatches();

    public void Draw()
    {
        foreach (var batch in batches)
            DrawBatch(batch);
    }

    private void DrawBatch(Batch batch)
    {
        Graphics.DrawMeshInstanced(batch.mesh, 0, batch.material, batch.transforms.ToArray());
    }

    [System.Serializable]
    public struct Batch
    {
        public Mesh mesh;
        public Material material;
        public List<Matrix4x4> transforms;

        public Batch(Mesh mesh = null, Material material = null, IEnumerable<Matrix4x4> transforms = null)
        {
            this.mesh = null;
            this.material = null;
            this.transforms = new List<Matrix4x4>(1000);
            AddRange(transforms);
        }

        public void Add(Matrix4x4 transform)
        {
            if (transforms.Count < 1000)
                transforms.Add(transform);
            else Debug.LogWarning("Trying to add more than 1000 meshes");
        }

        public void AddRange(IEnumerable<Matrix4x4> transforms)
        {
            if (transforms == null)
                return;
            foreach (var t in transforms)
                Add(t);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= transforms.Count)
                return;
            transforms.RemoveAt(index);
        }
    }
}