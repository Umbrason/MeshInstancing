using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleProfileEmitter : BaseEmitter
{
    public Mesh mesh;
    public Material material;
    public Vector3 originShift;
    public Vector3 baseScale;
    public Vector3 randomScaleRange;
    public Vector3 baseRotation;
    public Vector3 randomRotationRange;

    
    public Matrix4x4 ApplyBasePosAndRandomRotScale(System.Random random, Matrix4x4 TRS)
    {
        var scale = baseScale + Vector3.Scale(Utility.RandomVector(random), randomScaleRange);
        var rotation = Quaternion.Euler(baseRotation) * Quaternion.Euler(Vector3.Scale(Utility.RandomVector(random) * 2 - Vector3.one, randomRotationRange));
        return TRS * Matrix4x4.TRS(TRS.rotation * rotation * Vector3.Scale(scale, originShift), rotation, scale);
    }
}
