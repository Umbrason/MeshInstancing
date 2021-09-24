using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleProfileEmitter : BaseEmitter
{
    public Mesh mesh;
    public Material material;

    public Vector3 baseScale;
    public Vector3 randomScaleRange;
    public Vector3 baseRotation;
    public Vector3 randomRotationRange;

}
