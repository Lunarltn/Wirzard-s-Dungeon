using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightningStrike : SkillCast
{
    [SerializeField]
    HDAdditionalLightData _light;
    const string HIT_EFFECT = SUB_EFFECT_PATH + "ElectroHitEffect";
    const float RANGE = 2.5f;
    const float LIGHT_TIME = 0.2f;
    float _lightTime;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        _light.intensity = 400;
        _light.gameObject.SetActive(true);
        transform.position = skillInfo.stopTargetPos;
        _lightTime = Time.time;
        Collider[] colliders = Physics.OverlapSphere(transform.position, RANGE, (1 << skillTargetLayer));
        for (int i = 0; i < colliders.Length; i++)
        {
            HitTarget(colliders[i]);
            CreateHitEffect(colliders[i], HIT_EFFECT);
        }
    }

    protected override void UpdateSkill()
    {
        _light.intensity += Time.fixedDeltaTime * 2000;
        if (Time.time - _lightTime > LIGHT_TIME && _light.gameObject.activeSelf)
        {
            _light.gameObject.SetActive(false);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RANGE);
    }
}
