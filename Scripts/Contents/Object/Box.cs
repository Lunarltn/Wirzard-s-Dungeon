using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : DestructibleObject
{
    public override void Destroy(float force, Vector3 explosionPosition)
    {
        base.Destroy(force, explosionPosition);
    }

}
