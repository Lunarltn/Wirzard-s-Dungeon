using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : BaseController, IDamageable, IKnockbackable
{
    const string ATTACK_EFFECT = "Skill/SubEffect/AttackEffect";
    const float HIT_STOP_TIME = 0.6f;
    const float KNOCKBACK_STOP_TIME = 0.5f;
    const float KNOCKBACK_STOP_DISTANCE = 0.05f;

    public bool IsCompleteSpawn { get; private set; }
    public bool IsHitting => _isHitting;

    protected NavMeshAgent _agent;
    protected MonsterDetection _detection;
    protected MonsterStat _stat;
    Collider _collider;
    bool _isKnockback;
    bool _isHitting;

    CancellationTokenSource _cancelSource;

    protected override void OnEnable()
    {
        base.OnEnable();
        SpawnInit();
    }

    protected override void Init()
    {
        base.Init();
        _detection = GetComponent<MonsterDetection>();
        IsCompleteSpawn = true;
        InitStat();
        InitAgent();
        InitCollider();
    }

    void InitStat()
    {
        _stat = new MonsterStat(ID);
        baseStat = _stat;
    }

    void InitAgent()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent != null)
            _agent.speed = _stat.MoveSpeed;
    }

    void InitCollider()
    {
        _collider = Util.FindChild<Collider>(gameObject);
    }

    protected void SpawnInit()
    {
        IsDead = false;
        _isKnockback = false;
        AttackTarget = null;
        if (_collider != null)
            _collider.enabled = true;
        InitStat();
    }

    public void InitSpawnSetting(Vector3 spawnPosition)
    {
        GetComponent<NavMeshAgent>().Warp(spawnPosition);
        StartPosition = spawnPosition;
        IsCompleteSpawn = false;
        DOTween.Sequence()
            .OnStart(() =>
            {
                transform.localScale = Vector3.one * 0.1f;
            })
            .Append(transform.DOScale(1, 1))
            .OnComplete(() =>
            {
                IsCompleteSpawn = true;
            });
    }

    public override void Attack()
    {
        if (_detection.EnemyInDetectionRange != null)
        {
            var dir = _detection.EnemyInDetectionRange.transform.position - transform.position;
            Rotation(dir.normalized);
        }
        animator.SetBool(BATTLE_HASH, true);
        animator.SetTrigger(ATTACK_HASH);
    }

    //AnimationEvent
    public void AttackEvent()
    {
        if (_detection == null || _detection.DetectEnemyInAttackRange() == false)
            return;

        Collider hit = _detection.EnemyInAttackRange;
        Vector3 hitPoint = hit.ClosestPoint(hit.transform.position);
        HitTarget(hit);
        CreateHitEffect(hitPoint, ATTACK_EFFECT);
    }

    public virtual Damage TakeDamage(Damage damage)
    {
        damage = _stat.GetFinalDamage(damage);
        if (_stat.DecreaseHP(damage.Value) == false)
        {
            AttackTarget = damage.Caster;
            Die();
            damage.IsDie = true;
        }
        else
        {
            AttackTarget = damage.Caster;
            GetHit();
        }

        return damage;
    }

    protected override void Die()
    {
        base.Die();
        if (_agent.enabled == true)
            _agent.ResetPath();
        _collider.enabled = false;
        animator.SetTrigger(DIE_HASH);
        RemoveCorpseAfterDelay(5).Forget();

        if (AttackTarget != null && AttackTarget.gameObject.layer == Managers.Layer.PlayerLayer)
        {
            Managers.Quest.CountQuestRequest(Define.QuestCategory.Hunt, ID, 1);
            Managers.Inventory.DropItem(_stat.DropItem);
        }
    }

    async UniTask RemoveCorpseAfterDelay(float time)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        Managers.Resource.Destroy(gameObject);
    }

    protected virtual void GetHit()
    {
        animator.SetTrigger(HIT_HASH);
        Stiff().Forget();
    }

    async UniTask Stiff()
    {
        if (_agent == null || _detection == null || _isKnockback)
            return;

        float speed = 0;
        if (_agent.enabled)
        {
            speed = _agent.speed;
            _agent.speed = 0;
        }
        _isHitting = true;

        await UniTask.WaitForFixedUpdate();
        await UniTask.WaitUntil(() => _isKnockback == false);
        float time = Time.time;
        await UniTask.WaitUntil(() => Time.time > time + HIT_STOP_TIME);
        await UniTask.WaitForFixedUpdate();

        if (speed != 0)
            _agent.speed = speed;
        _isHitting = false;
    }

    public void GetKnockedBack(Vector3 force)
    {
        Knockback(force).Forget();
    }

    async UniTask Knockback(Vector3 force)
    {
        if (_agent == null)
            return;

        _agent.enabled = false;
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.AddForce(force, ForceMode.VelocityChange);
        _isKnockback = true;
        _cancelSource = new CancellationTokenSource();

        await UniTask.WaitForFixedUpdate(_cancelSource.Token);
        float time = Time.time;
        await UniTask.WaitUntil(() => rb.velocity.magnitude < KNOCKBACK_STOP_DISTANCE || Time.time > time + KNOCKBACK_STOP_TIME, cancellationToken: _cancelSource.Token);
        await UniTask.WaitForFixedUpdate(_cancelSource.Token);

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        _agent.Warp(transform.position);
        _agent.enabled = true;
        _isKnockback = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.TryGetComponent(out IKnockbackable knockbackable) && collision.gameObject.layer == gameObject.layer)
        {
            knockbackable.GetKnockedBack(-collision.impulse / Time.fixedDeltaTime);
        }
    }

    void Cancle()
    {
        _cancelSource?.Cancel();
    }

    void Dispose()
    {
        _cancelSource?.Dispose();
    }

    private void OnDisable()
    {
        Cancle();
        Dispose();
    }
}
