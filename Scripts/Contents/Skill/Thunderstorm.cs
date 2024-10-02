using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerInfoManager;

public class Thunderstorm : SkillCast
{
    const string PART_PATH = SKILL_PATH + "LightningStrike";
    const float RANGE = 4f;
    const int COUNT = 3;
    int _currentCount;
    float _minInterval = 0.15f;
    float _maxInterval = 0.35f;
    float _intervalTimer;
    float _height;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        _currentCount = COUNT;
        transform.position = skillInfo.stopTargetPos;

        var dir = (skillInfo.stopTargetPos - skillInfo.casterParent.position).normalized;
        //dir *= -1;
        if (Physics.Raycast(skillInfo.stopTargetPos - dir, Vector3.down, out RaycastHit hit, 100, Managers.Layer.GroundLayerMask))
            _height = hit.point.y;
    }

    protected override void UpdateSkill()
    {
        if (_currentCount <= 0)
            return;

        if (_intervalTimer > 0)
        {
            _intervalTimer -= Time.deltaTime;
            return;
        }

        GameObject skill = Managers.Resource.Instantiate(PART_PATH);
        SkillInfo skillInfo = this.skillInfo;
        skillInfo.stopTargetPos.y = _height;
        skillInfo.stopTargetPos += GetRandomVector(new Vector3(RANGE * 0.5f, 0, RANGE * 0.5f));
        skillInfo.duration = 1;
        skill.GetComponent<SkillCast>().Init(skillInfo, skillTarget);

        _intervalTimer = Random.Range(_minInterval, _maxInterval);
        _currentCount--;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, RANGE);
        if (Physics.BoxCast(
            transform.position + Vector3.up,
            Vector3.one / 2,
            Vector3.down,
            out RaycastHit hit,
            Quaternion.identity,
            50,
            Managers.Layer.GroundLayerMask))
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.down * hit.distance, Vector3.one);
        }
    }
}
