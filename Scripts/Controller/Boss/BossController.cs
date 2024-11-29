using Cysharp.Threading.Tasks;
using RootMotion.FinalIK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class BossController : BaseController
{
    readonly Vector3 IKTargetCorrectionValue = new Vector3(0, 1, 2);
    public int Phase { get { return _phase; } protected set { _phase = value; } }

    [SerializeField]
    protected Transform iKTarget;
    protected BossDetection detection;
    protected BossStat stat;
    protected NavMeshAgent agent;

    UI_BossStat _uIBossStat;
    LookAtIK _lookAtIK;
    List<BossColliderController> _bossColliders;
    int _phase;
    bool _isTakingDamage;
    float _damageTimer;

    protected override void Init()
    {
        base.Init();
        detection = GetComponent<BossDetection>();
        _lookAtIK = GetComponent<LookAtIK>();
        Phase = 0;

        InitStat();
        InitCollision();
        InitAgent();
        InitUIBossStat();
    }

    void Update()
    {
        LimitDamage();
        UpdateIKPosition();
        OnUpdating();
    }

    public void ResetHostility()
    {
        Phase = 0;
        CloseUIBossStat();
        stat.IncreaseHP(stat.HP);
        stat.RechargeShield();
    }

    public void Hostile()
    {
        Phase = 1;
        OpenUIBossStat();
    }

    void UpdateIKPosition()
    {
        if (_phase != 0 || IsDead == false)
        {
            IKLookAtTarget(Managers.PlayerInfo.Player.transform.position);
        }
    }
    protected virtual void OnUpdating() { }

    void InitStat()
    {
        stat = new BossStat(ID);
        baseStat = stat;
    }

    void InitAgent()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = stat.MoveSpeed;
    }

    void InitCollision()
    {
        _bossColliders = new List<BossColliderController>();
        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>())
        {
            var bcc = collider.gameObject.AddComponent<BossColliderController>();
            bcc.Init(TakeDamage);
            _bossColliders.Add(bcc);
        }
    }

    void LimitDamage()
    {
        if (_isTakingDamage)
            _damageTimer += Time.deltaTime;
        if (_damageTimer > 0.05f)
            _isTakingDamage = false;
    }

    void InitUIBossStat()
    {
        _uIBossStat = Managers.UI.ShowSceneUI<UI_BossStat>();
        _uIBossStat.SetUI(stat);
        _uIBossStat.SetBossName(stat.Name);
        _uIBossStat.gameObject.SetActive(false);
    }

    void OpenUIBossStat()
    {
        _uIBossStat?.UpdateHPStat(stat.HP, stat.CurrentHP);
        _uIBossStat?.UpdateShieldStat(stat.Shield, stat.CurrentShield);
        _uIBossStat?.gameObject.SetActive(true);
    }

    void CloseUIBossStat()
    {
        _uIBossStat?.gameObject.SetActive(false);
    }

    Damage TakeDamage(bool head, Damage damage)
    {
        if (_isTakingDamage)
        {
            damage.IsIgnored = true;
            return damage;
        }
        else _isTakingDamage = true;
        //크리티컬 여부
        damage.IsCritical = head;
        //데미지 계산
        damage = stat.GetFinalDamage(damage);
        //쉴드 감소
        stat.DecreaseShield(damage.Value);
        //체력감소
        if (stat.DecreaseHP(damage.Value) == false)
        {
            Die();
            damage.IsDie = true;
        }
        else
        {
            if (stat.CurrentShield == 0)
                GetHit();
        }
        //쉴드 리필
        if (stat.CurrentHP < stat.HP * 0.5f && Phase < 2)
        {
            Phase = 2;
            stat.RechargeShield();
        }

        return damage;
    }

    protected override void Die()
    {
        if (IsDead)
            return;

        detection.BossRoom.IsClear = true;
        agent.ResetPath();
        Managers.Inventory.DropItem(stat.DropItem);
        animator.SetTrigger(DIE_HASH);
        Managers.Quest.CountQuestRequest(Define.QuestCategory.Hunt, ID, 1);
        foreach (BossColliderController bcc in _bossColliders)
            bcc.Collider.enabled = false;
        CloseUIBossStat();
        base.Die();
    }

    void GetHit()
    {
        if (IsPlayingAttackTagAnimation() == false
            && animator.GetCurrentAnimatorStateInfo(0).IsName(HIT) == false)
            Stiff().Forget();
    }

    async UniTask Stiff()
    {
        float speed = agent.speed;
        agent.speed = 0;
        animator.SetTrigger(HIT_HASH);
        await UniTask.WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(HIT));
        await UniTask.WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).IsName(HIT));
        agent.speed = speed;

    }

    void OnIK(bool isLerp = false)
    {
        if (isLerp)
        {
            SetIKPositionWeightLerp(1).Forget();
        }
        else
            _lookAtIK.GetIKSolver().SetIKPositionWeight(1);
    }

    void OffIK(bool isLerp = false)
    {
        if (isLerp)
        {
            SetIKPositionWeightLerp(0).Forget();
        }
        else
            _lookAtIK.GetIKSolver().SetIKPositionWeight(0);
    }

    async UniTask SetIKPositionWeightLerp(float value)
    {
        bool isIncrease = _lookAtIK.GetIKSolver().GetIKPositionWeight() < value;

        while ((_lookAtIK.GetIKSolver().GetIKPositionWeight() < value && isIncrease)
            || (_lookAtIK.GetIKSolver().GetIKPositionWeight() > value && isIncrease == false))
        {
            if (isIncrease)
                _lookAtIK.GetIKSolver().SetIKPositionWeight(_lookAtIK.GetIKSolver().GetIKPositionWeight() + Time.deltaTime);
            else
                _lookAtIK.GetIKSolver().SetIKPositionWeight(_lookAtIK.GetIKSolver().GetIKPositionWeight() - Time.deltaTime);

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

    void Move(Vector3 dir, float speed)
    {
        Vector3 gravity = Vector3.up * rb.velocity.y;

        transform.position += dir * stat.MoveSpeed * speed * Time.fixedDeltaTime + gravity;
    }

    public override void Attack()
    {
        if (detection.EnemyInDetectionRange != null)
        {
            var dir = detection.EnemyInDetectionRange.transform.position - transform.position;
            Rotation(dir.normalized, true, 1.5f);
        }
        animator.SetTrigger(ATTACK_HASH);
    }


    public void IKLookAtTarget(Vector3 target)
    {
        iKTarget.position = target + IKTargetCorrectionValue;
    }

    public void IKResetTarget()
    {
        iKTarget.localPosition = IKTargetCorrectionValue;
    }

    private void OnDrawGizmos()
    {

    }
}