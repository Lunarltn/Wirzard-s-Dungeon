using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : FightState
{
    protected const string SFA = "SingleFrontAttack";
    protected const string CFA = "ContinuousFrontAttack";
    protected const string CA = "CastingAttack";
    const float CancleCooldownTime = 1;

    bool _isAttack;
    bool _cancleSkill;
    bool _isEndAttack;

    float _secondTimer;
    float _layerWeight;
    SkillHotKeySlot _currentSkillSlot;

    public AttackState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        _isAttack = false;
        _isEndAttack = false;
        _currentSkillSlot = Managers.HotKey.CurrentSkillSlot;
        _cancleSkill = _currentSkillSlot.SkillData.type == 2 || _currentSkillSlot.SkillData.type == 4;
        _secondTimer = 1;
        _layerWeight = 0;
        player.Animator.SetInteger(ATTACK_TYPE_HASH, _currentSkillSlot.SkillData.type);
    }

    public override void Exit()
    {
        player.Animator.SetInteger(ATTACK_TYPE_HASH, 0);
        if (_cancleSkill)
            _currentSkillSlot.RunCooldown(CancleCooldownTime);
        else
            _currentSkillSlot.RunCooldown();
        _currentSkillSlot = null;
        Managers.Input.PlayMove();
    }

    public override void LogicUpdate()
    {
        if (Managers.Input.MouseRightClick)
            Managers.Camera.ZoomInCamera();
        else
            Managers.Camera.ZoomOutCamera();


        AttackAnim((Define.SkillType)_currentSkillSlot.SkillData.type);
    }

    void AttackAnim(Define.SkillType type)
    {
        switch (type)
        {
            case Define.SkillType.SingleFront:
                if (IsPlayAnim(SFA, 1) && CurrentAnimTime(1) > 0.6f && !_isAttack)
                {
                    _isAttack = true;
                    player.CastSkill(_currentSkillSlot.SkillData);
                }
                if (IsPlayAnim(SFA, 1) && CurrentAnimTime(1) > 0.99f)
                {
                    stateMachine.ChangeState(player.FightState);
                }
                break;

            case Define.SkillType.ContinuousFront:
                Managers.Input.StopMove();
                if (IsPlayAnim(CFA, 0) && !_isAttack)
                {
                    player.CurrentWeaponTransform.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, -30), 0.2f);
                    player.Animator.SetLayerWeight(1, 0);
                    _isAttack = true;
                    player.CastSkill(_currentSkillSlot.SkillData);
                }

                if (StateTimer > 0.5f && _cancleSkill)
                {
                    _cancleSkill = false;
                }
                //스킬 시전 취소
                if (!Managers.Input.MouseLeftClick || _currentSkillSlot.SkillData.num != Managers.HotKey.CurrentSkillSlot.SkillData.num || _currentSkillSlot.SkillData.duration < StateTimer)
                {
                    _isEndAttack = true;
                    player.CurrentWeaponTransform.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, 0), 0.5f);
                    player.StopCasting();
                    player.Animator.SetInteger(ATTACK_TYPE_HASH, 0);
                }
                //1초마다 실행
                if (StateTimer - _secondTimer > 1)
                {
                    _secondTimer++;
                    //마나 부족
                    if (!player.Stat.DecreaseMP(Managers.HotKey.CurrentSkillSlot.SkillData.cost))
                    {
                        _isEndAttack = true;
                        player.CurrentWeaponTransform.DOLocalRotateQuaternion(Quaternion.Euler(0, 0, 0), 0.5f);
                        player.Animator.SetInteger(ATTACK_TYPE_HASH, 0);
                    }
                }
                //애니메이션 종료처리
                if (_isEndAttack)
                {
                    _layerWeight += Time.deltaTime * 2;
                    player.Animator.SetLayerWeight(1, _layerWeight);
                }

                if (_layerWeight >= 1)
                {
                    player.StopCasting();
                    stateMachine.ChangeState(player.FightState);
                }
                break;

            case Define.SkillType.SingleCasting:
                if (IsPlayAnim(CA, 1) && CurrentAnimTime(1) > 0.9f && !_isAttack)
                {
                    _isAttack = true;
                    player.CastSkill(_currentSkillSlot.SkillData);
                }
                if (IsPlayAnim(CA + "ing", 1) && CurrentAnimTime(1) > 0.99f)
                {
                    stateMachine.ChangeState(player.FightState);
                }
                break;
            case Define.SkillType.ContinuousCasting:
                Managers.Input.StopMove();
                if (IsPlayAnim(CA + "ing", 1) && CurrentAnimTime(1) <= 0.99f && !_isAttack)
                {
                    _isAttack = true;
                    player.CastSkill(_currentSkillSlot.SkillData);
                }

                if (StateTimer > 0.5f && _cancleSkill)
                {
                    _cancleSkill = false;
                }
                //스킬 시전 취소
                if (!Managers.Input.MouseLeftClick || _currentSkillSlot.SkillData.num != Managers.HotKey.CurrentSkillSlot.SkillData.num || _currentSkillSlot.SkillData.duration < StateTimer)
                {
                    _isEndAttack = true;
                    player.StopCasting();
                    player.Animator.SetInteger(ATTACK_TYPE_HASH, 0);
                }
                //1초마다 실행
                if (StateTimer - _secondTimer > 1)
                {
                    _secondTimer++;
                    //마나 부족
                    if (!player.Stat.DecreaseMP(Managers.HotKey.CurrentSkillSlot.SkillData.cost))
                    {
                        _isEndAttack = true;
                        player.Animator.SetInteger(ATTACK_TYPE_HASH, 0);
                    }
                }
                //애니메이션 종료처리
                if (_isEndAttack)
                {
                    player.StopCasting();
                    stateMachine.ChangeState(player.FightState);
                }
                break;
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void AnimationUpdate()
    {
        base.AnimationUpdate();
    }
}