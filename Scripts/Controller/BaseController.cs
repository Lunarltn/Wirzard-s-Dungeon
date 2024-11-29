using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public abstract class BaseController : MonoBehaviour
{
    protected readonly int MOVE_HASH = Animator.StringToHash("Move");
    protected readonly int ATTACK_HASH = Animator.StringToHash("Attack");
    protected readonly int HIT_HASH = Animator.StringToHash("Hit");
    protected readonly int BATTLE_HASH = Animator.StringToHash("Battle");
    protected readonly int SENSE_HASH = Animator.StringToHash("Sense");
    protected readonly int DEFEND_HASH = Animator.StringToHash("Defend");
    protected readonly int DIE_HASH = Animator.StringToHash("Die");
    protected const string HIT = "Hit";
    protected const string ATTACK_TAG = "Attack";
    protected Animator animator;
    protected Rigidbody rb;

    TweenerCore<Quaternion, Quaternion, NoOptions> _rotateTweener;
    CancellationTokenSource _cancleTokenSource;
    Transform _attackTarget;

    public int ID;
    public bool IsDead { get; protected set; }
    public BaseStat baseStat { get; protected set; }
    [HideInInspector]
    public Vector3 StartPosition;
    [HideInInspector]
    public Vector3 StartDirection;
    public PatrolDestinations PatrolDestinations;
    public Transform AttackTarget
    {
        get { return _attackTarget; }
        set
        {
            _attackTarget = value;
            if (_attackTarget != null)
            {
                Cancel();
                _cancleTokenSource = new CancellationTokenSource();
                CancleAttackTargetTimer(5).Forget();
            }
        }
    }

    protected virtual void OnEnable()
    {
        StartPosition = transform.position;
        StartDirection = transform.forward;
    }

    void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    public bool Rotation(Vector3 dir, bool lerp = true, float lerpSpeed = 0.5f, float completeAngle = 90)
    {
        if (dir == Vector3.zero)
            return true;

        if (_rotateTweener != null && _rotateTweener.active)
            _rotateTweener.Kill();
        if (lerp)
            _rotateTweener = transform.DORotateQuaternion(Quaternion.LookRotation(dir), lerpSpeed);
        else
            transform.rotation = Quaternion.LookRotation(dir);

        float differenceAngle = Vector3.Angle((transform.forward).normalized, dir);
        if (differenceAngle > completeAngle)
            return false;
        else
            return true;
    }

    virtual public void Attack() { }

    async UniTask CancleAttackTargetTimer(float time)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(time), cancellationToken: _cancleTokenSource.Token);
        _attackTarget = null;
    }

    protected void HitTarget(Collider collider)
    {
        IDamageable iDamage = Util.FindParent<IDamageable>(collider.gameObject);
        if (iDamage == null)
            return;

        Damage damage = new Damage()
        {
            Value = baseStat.Damage,
            Caster = transform
        };
        Damage fanalDamage = iDamage.TakeDamage(damage);
        var hitPoint = collider.ClosestPoint(transform.position);
        Managers.InfoUI.ShowDamageEffect(hitPoint, fanalDamage);
    }

    protected void CreateHitEffect(Vector3 hitPosition, string hitEffectName)
    {
        var hitEffect = Managers.Resource.Instantiate(hitEffectName);
        hitEffect.transform.position = hitPosition;
    }

    protected virtual void Die()
    {
        IsDead = true;
    }

    public bool IsPlayingAttackTagAnimation() => animator.GetCurrentAnimatorStateInfo(0).IsTag(ATTACK_TAG);

    void Cancel()
    {
        _cancleTokenSource?.Cancel();
    }

    void Dispose()
    {
        _cancleTokenSource?.Dispose();
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void OnDestroy()
    {
        _rotateTweener = null;
        Cancel();
        Dispose();
    }
}
