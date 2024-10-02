using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ExplosionTrap : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "TrapHitEffect";

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        transform.rotation = Quaternion.LookRotation((skillInfo.stopTargetPos - skillInfo.casterParent.position).normalized);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        var dir = (skillInfo.stopTargetPos - skillInfo.casterParent.position).normalized;
        dir *= -1;
        if (Physics.Raycast(skillInfo.stopTargetPos + dir, Vector3.down, out RaycastHit hit, 100, Managers.Layer.GroundLayerMask))
            skillInfo.stopTargetPos.y = hit.point.y;

        transform.position = skillInfo.stopTargetPos;
    }

    protected override void UpdateSkill()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.gameObject.layer == skillTargetLayer)
        {
            HitTarget(other);
            CreateHitEffect(other, HIT_EFFECT);
            Managers.Camera.PlayShake(10.0f, 0.5f);
            Managers.Resource.Destroy(gameObject);
        }
    }
}
