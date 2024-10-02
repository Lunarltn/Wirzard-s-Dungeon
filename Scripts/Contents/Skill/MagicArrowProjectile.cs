using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicArrowProjectile : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "MagicArrowHitEffect";
    const float MAX_DISTANCE = 0.7f;
    RaycastHit _hit;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        var randPos = GetRandomVector(skillInfo.casterParent.up + skillInfo.casterParent.right);
        transform.position = skillInfo.casterParent.position + randPos;
        transform.rotation = Quaternion.LookRotation(skillInfo.casterParent.forward);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        GetComponent<Rigidbody>().AddForce(skillInfo.casterParent.forward * skillInfo.speed, ForceMode.Impulse);
    }

    protected override void UpdateSkill()
    {
        if (Physics.Raycast(transform.position, transform.forward, out _hit, MAX_DISTANCE, ~excludedLayerMask))
        {
            var layer = _hit.transform.gameObject.layer;

            var hitEffect = CreateHitEffect(_hit, HIT_EFFECT);

            if (layer == skillTargetLayer)
            {
                HitTarget(_hit.collider);
                hitEffect.transform.parent = _hit.transform.GetChild(0);
            }
            Managers.Resource.Destroy(gameObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * MAX_DISTANCE);
    }
}

