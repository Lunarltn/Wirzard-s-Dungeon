using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Firebreath : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "FirebreathHitEffect";
    const float MAX_DISTANCSE = 17;
    const float DETECTION_RANGE = 2;
    const float TICK_DAMAGE_TIME = 0.5f;
    const float DURATION_TIME = 2;
    const float DIR_TIME = 3f;

    Vector3 _beforeDir;
    ParticleSystem _particleSystem;
    float _tickDamageTimer;
    float _endSkillTimer;
    float _dirTimer;
    bool _isEndSkill;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();

        _particleSystem.Stop();
        var main = _particleSystem.main;
        main.duration = skillInfo.duration;
        main.simulationSpeed = skillInfo.speed;
        _particleSystem.Play();
        currentDuration += DURATION_TIME;
        transform.position = skillInfo.casterParent.position;
        transform.rotation = Quaternion.LookRotation((skillInfo.moveTargetPos.Invoke() - skillInfo.casterParent.position).normalized);
        _tickDamageTimer = 1;
        _endSkillTimer = 0;
        _isEndSkill = false;

        InstantiateScorched("FirebreathScorched");
    }

    protected override void UpdateSkill()
    {
        Vector3 dir = (skillInfo.moveTargetPos.Invoke() - skillInfo.casterParent.position).normalized;
        if (Vector3.Distance(_beforeDir, dir) > 0.1f && _dirTimer >= DIR_TIME)
        {
            _beforeDir = dir;
            _dirTimer = 0;
        }
        if (_dirTimer < DIR_TIME)
            _dirTimer += Time.deltaTime * skillInfo.speed;

        Vector3 lerpDir = Vector3.Lerp(_beforeDir, dir, _dirTimer / DIR_TIME);

        transform.position = skillInfo.casterParent.position;
        transform.rotation = Quaternion.LookRotation(lerpDir);
        if ((skillInfo.isCastingSkill() == false || currentDuration - DURATION_TIME < 0) && _isEndSkill == false)
        {
            Stop();
        }

        if (_isEndSkill == false)
        {
            DetectionTarget(lerpDir);
        }
        else
            _endSkillTimer += Time.deltaTime;

        if (_endSkillTimer > DURATION_TIME)
            Managers.Resource.Destroy(gameObject);

    }

    void DetectionTarget(Vector3 dir)
    {
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, DETECTION_RANGE, dir, MAX_DISTANCSE, (1 << skillTargetLayer));
        RaycastHit scorchHit;
        if (Physics.Raycast(transform.position, dir, out scorchHit, MAX_DISTANCSE, scorchLayerMask))
        {
            if (DURATION_TIME + skillInfo.duration - 0.3f > currentDuration)
                EmitScorched(scorchHit.point, scorchHit.normal, 2, 2);
        }
        _tickDamageTimer += Time.deltaTime;
        if (_tickDamageTimer > TICK_DAMAGE_TIME)
        {
            _tickDamageTimer = 0;
            foreach (RaycastHit hit in hits)
            {
                HitTarget(hit.collider);
                CreateHitEffect(hit, HIT_EFFECT);
            }
        }
    }

    public override void Stop()
    {
        _particleSystem.Stop();
        _isEndSkill = true;
    }

}
