using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonBeam : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "PhotonHitEffect";

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        transform.position = skillInfo.stopTargetPos;
        HitTarget(skillInfo.targetCollider);
        CreateHitEffect(skillInfo.targetCollider, HIT_EFFECT);
    }

    protected override void UpdateSkill()
    {

    }
}
