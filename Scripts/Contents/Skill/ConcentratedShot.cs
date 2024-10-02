using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConcentratedShot : SkillCast
{
    const string PART_PATH = "Skill/ConcentratedShotMissile";
    const float INTERVAL_TO_SPEED = 2.5f;
    float _intervalTime;
    float _intervalTimer;
    int _shotCountEveryInterval;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        transform.position = skillInfo.casterParent.position;
        transform.rotation = Quaternion.LookRotation(skillInfo.stopTargetPos);
        _intervalTime = INTERVAL_TO_SPEED / skillInfo.speed * 10;
        _shotCountEveryInterval = 1;
    }

    protected override void UpdateSkill()
    {
        if (_intervalTimer > 0)
        {
            _intervalTimer -= Time.deltaTime;
            return;
        }
        if (currentDuration < skillInfo.duration * 0.4f && _shotCountEveryInterval == 1)
            _shotCountEveryInterval = 2;
        else if (currentDuration < skillInfo.duration * 0.7f && _shotCountEveryInterval == 2)
            _shotCountEveryInterval = 3;

        for (int i = 0; i < Random.Range(1, _shotCountEveryInterval + 1); i++)
        {
            GameObject skill = Managers.Resource.Instantiate(PART_PATH);
            var missile = skill.GetComponent<ConcentratedShotMissile>();
            missile.Init(skillInfo, skillTarget);
            missile.ChangeRimcolorStrength((skillInfo.duration - currentDuration) * 15);
            missile.ChangeSize((skillInfo.duration - currentDuration) / (skillInfo.duration * 12) + 0.1f);
        }
        var speed = INTERVAL_TO_SPEED / skillInfo.speed;
        _intervalTimer = Mathf.Max(_intervalTime -= speed * (speed * 12), speed);
    }

    public override void Stop()
    {
        Managers.Resource.Destroy(gameObject);
    }
}

