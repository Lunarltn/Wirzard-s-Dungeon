using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class PlayerController : BaseController, IDamageable, IKnockbackable
{
    PlayerDetection _detection;
    public PlayerDetection Detection => _detection;
    float _runSpeed = 1;
    public float RunSpeed
    {
        get { return _runSpeed; }
        set { _runSpeed = Mathf.Clamp(value, 1, 2); }
    }

    [SerializeField]
    float _jumpForce = 6;

    public PlayerState AttackState;
    public PlayerState CrouchState;
    public PlayerState DodgeState;
    public PlayerState FightState;
    public PlayerState WalkState;
    public PlayerState JumpState;
    public PlayerState DieState;
    public PlayerState SpecialActionState;
    public PlayerState KickAttackState;


    StateMachine stateMachine;

    PlayerStat _stat;
    public PlayerStat Stat
    {
        get
        {
            if (_stat == null)
                _stat = new PlayerStat();
            return _stat;
        }
    }

    [SerializeField]
    float _dodgeLange = 500;
    const float KICK_FORCE = 15;

    [SerializeField]
    Transform _weaponTransform;

    public Transform CurrentWeaponTransform
    {
        get
        {
            for (int i = 0; i < _weaponTransform.childCount; i++)
                if (_weaponTransform.GetChild(i).gameObject.activeSelf)
                    return _weaponTransform.GetChild(i);
            return null;
        }
    }
    public Transform CurrentWeaponCastingTransform => CurrentWeaponTransform?.GetChild(0);

    const float KNOCKBACK_STOP_TIME = 0.5f;
    const float KNOCKBACK_STOP_DISTANCE = 0.05f;
    public Animator Animator => animator;
    CapsuleCollider _collider;
    SkillCast _castingSkill;
    bool _isKnockback;

    protected override void Init()
    {
        base.Init();
        _detection = GetComponent<PlayerDetection>();
        _collider = GetComponent<CapsuleCollider>();

        stateMachine = new StateMachine();
        AttackState = new AttackState(this, stateMachine);
        CrouchState = new CrouchState(this, stateMachine);
        DodgeState = new DodgeState(this, stateMachine);
        FightState = new FightState(this, stateMachine);
        WalkState = new WalkState(this, stateMachine);
        JumpState = new JumpState(this, stateMachine);
        DieState = new DieState(this, stateMachine);
        SpecialActionState = new SpecialActionState(this, stateMachine);
        KickAttackState = new PlayerKickAttackState(this, stateMachine);

        stateMachine.Init(WalkState);

        Managers.Inventory.SetEquipment(Define.EquipCategory.Weapon, new Item(1001, 1));
    }

    void Update()
    {
        stateMachine.CurrentState.LogicUpdate();
        stateMachine.CurrentState.AnimationUpdate();
        stateMachine.CurrentState.LogicTimer();

        DetectInterection();
    }

    void FixedUpdate()
    {
        stateMachine.CurrentState.PhysicsUpdate();
    }

    public Damage TakeDamage(Damage damage)
    {
        if (stateMachine.CurrentState == DodgeState)
            return new Damage();

        damage = _stat.GetFinalDamage(damage);
        if (_stat.DecreaseHP(damage.Value) == false)
        {
            Die();
            damage.IsDie = true;
        }
        else
        {
            if (damage.IsCritical)
                GetHit();
        }

        return damage;
    }
    public Action SpecialAction;
    public Action SpecialActionAnimation;

    public void CommandMoveToDestinationState(Vector3 destination, Vector3 endDir)
    {
        destination = new Vector3(destination.x, transform.position.y, destination.z);
        SpecialAction = () =>
        {
            if (MoveToDestination(destination, 0.3f))
                Rotation(endDir);
        };
        SpecialActionAnimation = () =>
        {
            if (Vector3.Distance(transform.position, destination) > 0.3f)
                animator.SetFloat(MOVE_HASH, 1);
            else animator.SetFloat(MOVE_HASH, 0);
        };
        stateMachine.ChangeState(SpecialActionState);
    }

    public void ResetState()
    {
        stateMachine.ChangeState(WalkState);
    }

    public void Respawn(Transform point)
    {
        IsDead = false;
        transform.position = point.position;
        transform.rotation = point.rotation;
        if (_stat.CurrentHP != _stat.HP)
            ReceiveRecoveryHP(_stat.HP);
        if (_stat.CurrentMP != _stat.MP)
            ReceiveRecoveryMP(_stat.MP);
        animator.SetBool(DIE_HASH, false);
        ResetState();
    }

    protected override void Die()
    {
        if (IsDead)
            return;
        IsDead = true;
        Managers.PlayerInfo.DiePlayer();
        stateMachine.ChangeState(DieState);
        animator.SetBool(DIE_HASH, true);
    }

    public bool ReceiveRecoveryHP(int amount)
    {
        return _stat.IncreaseHP(amount);
    }

    public bool ReceiveRecoveryMP(int amount)
    {
        return _stat.IncreaseMP(amount);
    }

    void GetHit()
    {
        animator.SetTrigger(HIT_HASH);
    }

    public void ChangeWeapon(Item item)
    {
        for (int i = 0; i < _weaponTransform.childCount; i++)
        {
            _weaponTransform.GetChild(i).gameObject.SetActive(false);
            if (item != null && _weaponTransform.GetChild(i).gameObject.name == item.Num.ToString())
                _weaponTransform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public bool MoveToDestination(Vector3 destination, float distance)
    {
        if (Vector3.Distance(transform.position, destination) > distance)
        {
            //감속
            float runSpeed = Vector3.Distance(transform.position, destination) - distance;
            if (runSpeed < 1)
                _runSpeed = runSpeed + 0.2f;
            //이동
            Vector3 dir = (destination - transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z);
            if (Rotation(dir))
                Move(dir);
            return false;
        }
        rb.velocity = Vector3.zero;
        return true;
    }


    public void Move(Vector3 dir)
    {
        if (_isKnockback)
            return;

        Vector3 gravity = Vector3.up * rb.velocity.y;
        //경사
        if (_detection.IsOnGround && _detection.IsOnSlope && !Managers.Input.IsJump)
        {
            gravity = Vector3.zero;
            dir = _detection.DirectionToSlope(dir);
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }

        if (_detection.IsOnMarginalSlope == false)
        {
            rb.velocity = dir * Stat.MoveSpeed * _runSpeed * Time.fixedDeltaTime + gravity;
        }
    }

    public void Jump()
    {
        rb.useGravity = true;
        if (!Managers.Input.IsJump || !_detection.IsOnGround || _detection.IsOnMarginalSlope)
            return;
        rb.velocity = Vector3.ProjectOnPlane(rb.velocity, transform.up) * (_jumpForce / 2) + transform.up * _jumpForce;
    }

    public void NormalMove(Vector3 moveDirection)
    {
        Vector3 nextDir = MoveToCameraDirection(moveDirection);
        if (Rotation(nextDir, true, 0.3f))
            Move(nextDir);

    }

    public void FightMove(Vector3 moveDirection)
    {
        Vector3 nextDir = MoveToCameraDirection(moveDirection);
        if (Rotation(Managers.Camera.CameraDir, true, 0.3f))
            Move(nextDir);
    }

    public void CrouchMove(Vector3 moveDirection)
    {
        FightMove(moveDirection);
    }

    public void Dodge(Vector3 moveDirection)
    {
        Vector3 nextDir = _detection.DirectionToSlope(MoveToCameraDirection(moveDirection));
        rb.velocity = nextDir * _dodgeLange * Time.fixedDeltaTime;
    }

    public Vector3 MoveToCameraDirection(Vector3 moveDirection)
    {
        if (moveDirection == Vector3.zero)
            return Vector3.zero;

        var dir = Managers.Camera.CameraDir * moveDirection.z;
        dir += Quaternion.Euler(0, 90, 0) * Managers.Camera.CameraDir * moveDirection.x;
        return dir.normalized;
    }

    public void StopCasting()
    {
        _castingSkill?.Stop();
    }

    public void CastSkill(SkillData skillData)
    {
        _castingSkill = Managers.Resource.Instantiate($"Skill/{skillData.prefeb}").GetComponent<SkillCast>();
        SkillInfo skillInfo;
        if (skillData.type == (int)Define.SkillType.SingleCasting || skillData.type == (int)Define.SkillType.ContinuousCasting)
            skillInfo = GetSkillInfo(skillData, transform);
        else
            skillInfo = GetSkillInfo(skillData, CurrentWeaponCastingTransform);
        _castingSkill.Init(skillInfo, Define.SkillTarget.Monster);
    }

    public void KickAttack()
    {
        for (int i = 0; i < _detection.DetectKickRange(); i++)
        {
            var go = _detection.KickedEnemyColliders[i].gameObject;
            Util.FindParent<IKnockbackable>(go)?.GetKnockedBack(KICK_FORCE * transform.forward);
            Damage damage = Util.FindParent<IDamageable>(go)?.TakeDamage(new Damage() { Value = 50, Caster = transform }) ?? new Damage();
            Managers.InfoUI.ShowDamageEffect(_detection.KickedEnemyColliders[i].ClosestPoint(go.transform.position), damage);
        }
    }

    public void Crouch(bool isCrouch)
    {
        float standCenter = 0.9f;
        float crouchCenter = 0.65f;
        float standHeight = 1.75f;
        float crouchHeight = 1.2f;
        if (isCrouch)
        {
            _collider.center = new Vector3(0, crouchCenter, 0);
            _collider.height = crouchHeight;
        }
        else
        {
            _collider.center = new Vector3(0, standCenter, 0);
            _collider.height = standHeight;
        }
    }

    public void DetectInterection()
    {
        if (_detection.DetectInteractionRange()
            && Managers.Quest.IsOpenQuestWindow == false
            && _detection.GetINearbyInteraction() != null
            && GetInterectionName() != string.Empty)
            Managers.InfoUI.ShowInterectionMessege(GetInterectionName(), GetInterectionMessage(), Interect);
        else
            Managers.InfoUI.HideInterectionMessege();
    }

    void Interect()
    {
        _detection.GetINearbyInteraction()?.SetPosition(transform.position);
        _detection.GetINearbyInteraction()?.Interect();
    }

    string GetInterectionName()
    {
        return _detection.GetINearbyInteraction()?.GetInterectionName();
    }

    string GetInterectionMessage()
    {
        return _detection.GetINearbyInteraction()?.GetInterectionMessage();
    }

    public SkillInfo GetSkillInfo(SkillData skillData, Transform casterParent)
    {
        SkillInfo skillInfo = new SkillInfo()
        {
            isCastingSkill = Managers.Input.IsMouseLeftClick,
            casterTransform = transform,
            casterParent = casterParent,
            speed = skillData.speed,
            duration = skillData.duration,
            damage = Managers.PlayerInfo.Skill.GetSkillDamage((Define.SkillName)skillData.num),
            stopTargetPos = Managers.Camera.DetectAimPointRayPosition()
        };

        return skillInfo;
    }

    public SkillInfo GetSkillInfo(SkillData skillData, Transform casterTransform, Vector3 targetPos)
    {
        SkillInfo skillInfo = GetSkillInfo(skillData, casterTransform);
        skillInfo.stopTargetPos = targetPos;
        return skillInfo;
    }

    public void GetKnockedBack(Vector3 force)
    {
        Knockback(force).Forget();
    }

    async UniTask Knockback(Vector3 force)
    {
        _isKnockback = true;
        rb.AddForce(force, ForceMode.VelocityChange);
        await UniTask.WaitForFixedUpdate();
        float time = Time.time;
        await UniTask.WaitUntil(() => rb.velocity.magnitude < KNOCKBACK_STOP_DISTANCE || Time.time > time + KNOCKBACK_STOP_TIME);
        await UniTask.WaitForFixedUpdate();
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        _isKnockback = false;
    }


    //IK
    protected const string IKLEFTFOOT = "IKLeftFootWeight";
    protected const string IKRIGHTFOOT = "IKRightFootWeight";
    /*
    [Range(0, 1), SerializeField]
    float distanceToGround;
    const int _groundLayer = 1 << 6;
    private void OnAnimatorIK(int layerIndex)
    {
        if (Anim.GetCurrentAnimatorStateInfo(0).IsName("Move Blend"))
        {
            Anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, Anim.GetFloat(IKLEFTFOOT));
            Anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, Anim.GetFloat(IKLEFTFOOT));
            Anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, Anim.GetFloat(IKRIGHTFOOT));
            Anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, Anim.GetFloat(IKRIGHTFOOT));

            Ray ray = new Ray(Anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit hit, distanceToGround + 1f, _groundLayer))
            {
                if (hit.transform.tag == "Ground")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;
                    Anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPosition);
                    Anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }

            ray = new Ray(Anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, distanceToGround + 1f, _groundLayer))
            {
                if (hit.transform.tag == "Ground")
                {
                    Vector3 footPosition = hit.point;
                    footPosition.y += distanceToGround;
                    Anim.SetIKPosition(AvatarIKGoal.RightFoot, footPosition);
                    Anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            }
        }

    }*/
}
