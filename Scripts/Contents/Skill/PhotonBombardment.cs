using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonBombardment : SkillCast
{
    const string PART_PATH = SKILL_PATH + "PhotonBeam";
    const float RANGE = 8f;
    const float INTERVAL_TIME = 0.5f;
    const int ARRAY_RANGE = 10;

    ParticleSystem _particleSystem;
    Collider[] _targets;
    float _interval;
    bool _isStop;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        _targets = new Collider[ARRAY_RANGE];
        base.Init(skillInfo, skillTarget);

        _particleSystem = GetComponent<ParticleSystem>();
        transform.position = skillInfo.casterParent.position;
        _isStop = false;
    }

    protected override void UpdateSkill()
    {
        if (_isStop)
            return;

        if (_interval > 0)
        {
            _interval -= Time.deltaTime;
            return;
        }
        int count = Physics.OverlapSphereNonAlloc(transform.position, RANGE, _targets, skillTargetLayerMask);
        if (count > 0)
        {
            Managers.Camera.PlayShake(3, 1);
            for (int i = 0; i < count; i++)
            {
                GameObject skill = Managers.Resource.Instantiate(PART_PATH);
                SkillInfo skillInfo = this.skillInfo;
                skillInfo.targetCollider = _targets[i];
                skillInfo.stopTargetPos = _targets[i].transform.position;
                skill.GetComponent<SkillCast>().Init(skillInfo, skillTarget);
            }
        }
        _particleSystem.Emit(1000);

        _interval = INTERVAL_TIME;
    }

    public override void Stop()
    {
        _isStop = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RANGE);
    }
}
