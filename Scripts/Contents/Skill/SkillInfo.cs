using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SkillInfo
{
    public Transform casterTransform;
    public Transform mouseTarget => Managers.Camera.DetectAimPointRayTarget();
    public Func<Vector3> moveTargetPos => () => Managers.Camera.DetectAimPointRayPosition();
    public Collider targetCollider;
    public Vector3 stopTargetPos;
    public Func<bool> isCastingSkill;
    public Transform casterParent;
    public float speed;
    public float duration;
    public int damage;
}
