using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaBubble : SkillCast
{
    const string HIT_EFFECT = "Skill/FirebreathHitEffect";
    const string SkillRangeName = "SkillCircleRange";
    const string SkillEffectName = "SkillEffect";

    GameObject _skillRange;
    GameObject _skillEffect;
    SphereCollider _collider;

    const float DelayTime = 1f;

    float _delayTimer;
    bool _isHit;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        if (_collider == null)
            _collider = GetComponent<SphereCollider>();
        if (_skillRange == null)
            _skillRange = Util.FindChild(gameObject, SkillRangeName);
        if (_skillEffect == null)
            _skillEffect = Util.FindChild(gameObject, SkillEffectName);

        _collider.radius = 0;
        _delayTimer = DelayTime;
        _skillRange.SetActive(true);
        _skillEffect.SetActive(false);
        transform.position = skillInfo.stopTargetPos;
        _isHit = false;
    }

    protected override void UpdateSkill()
    {
        if (_collider.radius <= 2.5f && _delayTimer < 0)
        {
            if (_skillEffect.activeSelf == false)
            {
                _skillEffect.SetActive(true);
                _skillRange.SetActive(false);
            }
            _collider.radius += Time.deltaTime * 9;
        }
        else
            _delayTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isHit) return;

        if (other.transform.gameObject.layer == skillTargetLayer)
        {
            _isHit = true;
            HitTarget(other);
        }
    }

    /*void HitTarget(Collider collider)
    {
        IDamageable iDamage = Util.FindParent<IDamageable>(collider.gameObject);
        if (iDamage == null) return;
        var hitPoint = collider.ClosestPoint(transform.position);
        Damage damage = iDamage.TakeDamage(new Damage(skillInfo.damage));
        TextEffect(hitPoint, damage);
        var hitEffect = Managers.Resource.Instantiate(HIT_EFFECT);
        hitEffect.transform.position = hitPoint;
    }*/
}
