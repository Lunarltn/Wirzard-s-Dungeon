using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire : SkillCast
{
    readonly Vector3 _localPosition = new Vector3(1.2f, 2.0f, 0f);
    const float DELAY_TIME = 0.5f;
    Vector3 _beforePosition;
    float _beforeYAngle;
    float _delayTimer;
    bool _isMoveCaster;
    Rigidbody _rigidbody;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        transform.position = skillInfo.casterParent.position + _localPosition;
        _beforeYAngle = skillInfo.casterParent.rotation.eulerAngles.y;
        _delayTimer = 0;
        _beforePosition = Vector3.zero;
    }

    protected override void UpdateSkill()
    {
        if (Mathf.Abs(_beforeYAngle - skillInfo.casterParent.rotation.eulerAngles.y) > 0.1f ||
            Vector3.Distance(_beforePosition, skillInfo.casterParent.position) > 0.1f)
        {
            _delayTimer += Time.deltaTime;
            if (_delayTimer > DELAY_TIME)
                _isMoveCaster = true;
        }
        else
            _delayTimer = 0;

        if (_isMoveCaster)
        {
            _beforeYAngle = Mathf.Lerp(_beforeYAngle, skillInfo.casterParent.rotation.eulerAngles.y, Time.deltaTime * 3);
            _beforePosition = Vector3.Lerp(_beforePosition, skillInfo.casterParent.position, Time.deltaTime * 3);
            var target = (Quaternion.Euler(0, _beforeYAngle, 0) * _localPosition) + skillInfo.casterParent.position;
            if (Vector3.Distance(transform.position, target) > 0.1f)
            {
                _rigidbody.position = Vector3.Lerp(_rigidbody.position, target, Time.deltaTime * skillInfo.speed);
                _isMoveCaster = false;
            }
        }
        else
            _rigidbody.velocity = Vector3.zero;
    }
}//회전,포지션 검사>0.5초경과>팔로우
//회전한뒤 0.5초뒤 불빛이 특정위치로 따라오게
