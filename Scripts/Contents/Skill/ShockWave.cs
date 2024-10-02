using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWave : SkillCast
{
    const float SKILL_RANGE = 11f;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        transform.position = GetFloorHeight(skillInfo.stopTargetPos);
        transform.GetChild(0).gameObject.SetActive(false);
        CreateSkillRange(transform.position, SKILL_RANGE, 1.1f);
    }


    protected override void UpdateSkill()
    {

    }

    public void Wave()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, SKILL_RANGE, skillTargetLayerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            HitTarget(colliders[i]);
            var force = ((colliders[i].ClosestPoint(transform.position) - transform.position).normalized + Vector3.up) * 15;
            colliders[i].transform.GetComponent<IKnockbackable>().GetKnockedBack(force);
        }

        Managers.Camera.PlayShake(20, 0.8f);
        transform.GetChild(0).gameObject.SetActive(true);
        currentDuration = 3;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, SKILL_RANGE);
    }
}
