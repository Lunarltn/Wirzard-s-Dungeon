using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UIElements;

public class GolemController : BossController
{
    const string THROWING_ATTACK_PATH = "Skill/GolemStoneBall";
    const string JUMPING_ATTACK_PATH = "Skill/ShockWave";
    const string ATTACK_EFFECT = "Skill/SubEffect/BossAttackEffect";
    const string TROWING_ATTACK = "ThrowingAttack";
    const string JUMPING_ATTACK = "JumpingAttack";
    readonly int TROWING_ATTACK_HASH = Animator.StringToHash("ThrowingAttack");
    readonly int JUMPING_ATTACK_HASH = Animator.StringToHash("JumpingAttack");

    const float JUMPING_ATTACK_TIMING = 0.7f;
    [SerializeField]
    Transform attackTarget;
    [SerializeField]
    Transform throwingAttackTarget;
    GolemStoneBall _stoneBall;
    ShockWave _shockWave;

    public override void Attack()
    {
        if (IsPlayingAttackTagAnimation())
            return;
        base.Attack();

    }

    public void ThrowAttack()
    {
        if (IsPlayingAttackTagAnimation())
            return;
        animator.ResetTrigger(HIT_HASH);
        animator.SetTrigger(TROWING_ATTACK_HASH);

        Rotation(detection.GetPlayerDirection());

        var stone = Managers.Resource.Instantiate(THROWING_ATTACK_PATH);
        SkillInfo skillInfo = new SkillInfo()
        {
            casterTransform = transform,
            casterParent = throwingAttackTarget.transform,
            targetCollider = Managers.PlayerInfo.Player.GetComponent<Collider>(),
            stopTargetPos = iKTarget.transform.position,
            duration = 30,
            speed = 1.5f,
            damage = stat.Damage
        };
        _stoneBall = stone.GetComponent<GolemStoneBall>();
        _stoneBall?.Init(skillInfo, Define.SkillTarget.Player);
    }

    public void JumpAttack()
    {
        if (IsPlayingAttackTagAnimation())
            return;
        animator.ResetTrigger(HIT_HASH);
        animator.SetTrigger(JUMPING_ATTACK_HASH);

        Rotation(detection.GetPlayerDirection());
        MoveJumpingAttack().Forget();

        var shockWave = Managers.Resource.Instantiate(JUMPING_ATTACK_PATH);
        SkillInfo skillInfo = new SkillInfo()
        {
            stopTargetPos = Managers.PlayerInfo.Player.transform.position,
            duration = 30,
            speed = 1.2f,
            damage = stat.Damage
        };
        _shockWave = shockWave.GetComponent<ShockWave>();
        _shockWave?.Init(skillInfo, Define.SkillTarget.Player);
    }


    async UniTask MoveJumpingAttack()
    {
        Vector3[] points = new Vector3[2];
        points[0] = transform.position;
        points[1] = Managers.PlayerInfo.Player.transform.position;
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(JUMPING_ATTACK));

        while (animator.GetCurrentAnimatorStateInfo(0).IsName(JUMPING_ATTACK) &&
            animator.GetCurrentAnimatorStateInfo(0).normalizedTime < JUMPING_ATTACK_TIMING)
        {
            var t = animator.GetCurrentAnimatorStateInfo(0).normalizedTime / JUMPING_ATTACK_TIMING;
            transform.position = Vector3.Lerp(points[0], points[1], t);

            await UniTask.WaitForFixedUpdate();
        }
    }

    //AnimationEvent
    public void JumpingAttackEvent()
    {
        _shockWave?.Wave();
        _stoneBall = null;
    }
    //AnimationEvent
    public void ThorwingAttackEvent()
    {
        _stoneBall?.Thorw();
        _stoneBall = null;
    }
    //AnimationEvent
    float _attackRange = 3.0f;
    public void AttackEvent()
    {
        if (detection == null || detection.EnemyInAttackRange == null)
            return;

        Collider[] colliders = Physics.OverlapSphere(attackTarget.position, _attackRange, Managers.Layer.PlayerLayerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            HitTarget(colliders[i]);
            CreateHitEffect(colliders[i].ClosestPoint(transform.position), ATTACK_EFFECT);
            var force = (colliders[i].ClosestPoint(transform.position) - transform.position).normalized * 20;
            colliders[i].transform.GetComponent<IKnockbackable>().GetKnockedBack(force);
        }

    }

    private void OnDrawGizmos()
    {
        if (attackTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackTarget.position, _attackRange);
        }
    }
}
