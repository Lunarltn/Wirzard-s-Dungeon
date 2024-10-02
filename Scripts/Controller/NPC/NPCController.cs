using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static TranslateScript;

public class NPCController : BaseController, IInteraction, IDamageable
{
    enum BTState
    {
        None,
        Die,
        Recover,
        Talk,
        Battle,
        MoveToDestination,
        FollowPlayer
    }
    const string ATTACK_EFFECT = "Skill/SubEffect/AttackEffect";
    const string InterectionMessage_Talk = "대화";
    const string InterectionMessage_Recover = "부활";

    const float HIT_STOP_TIME = 0.6f;
    const float TalkBubbleFlowTime = 5;
    const float RecoverTime = 5;
    const float RunSpeed = 2;
    const float PlayerStoppingDistance = 3;
    const float AttackDistance = 2.3f;
    const float HitTime = 1;
    const float StopMovingTime = 2;
    public Vector3 InterectionDir { get; private set; }
    public bool IsTalking => _isTalking;

    [SerializeField]
    Transform neck;

    NavMeshAgent _agent;
    NPCDetection _detection;
    NPCStat _stat;
    BTState _BTState;
    bool _isRecovering;
    bool _isTalking;
    float _stopMovingNearEnemyTimer;
    float _recoverTimer;
    float _moveAnimTimer;
    float _stopMovingTimer;
    float _attackTimer;
    float _hitTimer;

    protected override void Init()
    {
        base.Init();
        if (neck == null)
            neck = Util.FindChild<Transform>(transform.gameObject, "neck_01", true);
        _detection = GetComponent<NPCDetection>();
        Managers.NPC.SetNPC(ID, transform);

        InitStat();
        InitAgent();
    }

    void InitStat()
    {
        _stat = new NPCStat(ID);
        baseStat = _stat;
    }

    void InitAgent()
    {
        _agent = GetComponent<NavMeshAgent>();
        if (_agent != null)
            _agent.speed = _stat.MoveSpeed;
    }

    public override void Attack()
    {
        if (_detection.EnemyInDetectionRange != null)
        {
            var dir = _detection.EnemyInDetectionRange.transform.position - transform.position;
            Rotation(dir.normalized);
        }
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


    public Damage TakeDamage(Damage damage)
    {
        damage = _stat.GetFinalDamage(damage);
        if (_stat.DecreaseHP(damage.Value) == false)
        {
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
        _agent.ResetPath();
        animator.SetBool(DIE_HASH, true);
        AttackTarget = null;
    }

    void GetHit()
    {
        animator.SetTrigger(HIT_HASH);
        Stiff().Forget();
    }

    async UniTask Stiff()
    {
        if (_agent == null || _detection == null)
            return;

        float speed = 0;
        if (_agent.enabled)
        {
            speed = _agent.speed;
            _agent.speed = 0;
        }

        await UniTask.WaitForFixedUpdate();
        float time = Time.time;
        await UniTask.WaitUntil(() => Time.time > time + HIT_STOP_TIME);
        await UniTask.WaitForFixedUpdate();

        if (speed != 0)
            _agent.speed = speed;
    }


    void Recover()
    {
        _isRecovering = true;
        IsInRecovery(RecoverTime).Forget();
        Managers.NPC.ShowNPCRecoverTime(ID, RecoverTime);
    }

    async UniTask IsInRecovery(float time)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(time));
        IsDead = false;
        animator.SetBool(DIE_HASH, false);
        _stat.IncreaseHP(_stat.HP);
    }

    #region Interection
    public void Interect()
    {
        if (_isRecovering)
            return;
        if (IsDead && _isRecovering == false)
        {
            Recover();
            return;
        }

        Rotation(InterectionDir);
        if (Managers.Quest.CanStartQuestIDAtNPCID(ID) != 0)
        {
            _isTalking = true;
            Managers.Quest.OpenQuestWindow(transform, neck, ID, CloseQuestWindow);
        }
        else if (Managers.Quest.CanFinishQuestIDAtNPCID(ID) != 0)
        {
            int questID = Managers.Quest.CanFinishQuestIDAtNPCID(ID);
            Managers.Quest.CompleteQuest(questID);
            Managers.NPC.ShowTalkBubble(ID, Managers.Data.NPCDic[ID].completeQuest, TalkBubbleFlowTime);
        }
        else
            Managers.NPC.ShowTalkBubble(ID, Managers.Data.NPCDic[ID].talk, TalkBubbleFlowTime);

    }

    public string GetInterectionName()
    {
        return Managers.Data.NPCDic[ID].name;
    }

    public string GetInterectionMessage()
    {
        if (_detection.DetectEnemyInDetectionRange() || _isRecovering)
            return string.Empty;
        if (IsDead)
            return InterectionMessage_Recover;
        return InterectionMessage_Talk;
    }

    public void SetPosition(Vector3 position)
    {
        InterectionDir = (position - transform.position).normalized;
    }

    void CloseQuestWindow()
    {
        ResetRotation();
        _isTalking = false;
    }

    public void ResetRotation()
    {
        Rotation(StartDirection);
    }
    #endregion

    INode SettingBT()
    {
        return new SelectorNode
        (
            new List<INode>()
            {//부활
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckRecovering),
                        new ActionNode(WaitRecovering)
                    }
                ),//사망
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckDead),
                    }
                ),//피격
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckHitting),
                        new ActionNode(TakeHit)
                    }
                ),//대화
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckTalking),
                        new ActionNode(Talk)
                    }
                ),//적발견
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckDetectedEnemy),
                        new SelectorNode
                        (
                            new List<INode>()
                            {
                                new SequenceNode
                                (
                                    new List<INode>()
                                    {
                                        new ActionNode(CheckEnemyDistance),
                                        new ActionNode(StopAttack),
                                        new ActionNode(Attacki),
                                    }
                                ),
                                new SequenceNode
                                (
                                    new List<INode>()
                                    {
                                        new ActionNode(StopMovingNearEnemy),
                                        new ActionNode(MoveNearEnemy)
                                    }
                                )
                            }
                        )
                    }
                ),//플레이어추적
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckFollowPlayer),
                        new SelectorNode
                        (
                            new List<INode>()
                            {
                                new ActionNode(StopFollowing),
                                new ActionNode(FollowPlayer)
                            }
                        )
                    }
                ),//정찰
                new SequenceNode
                (
                    new List<INode>()
                    {
                        new ActionNode(CheckMoveToDestination),
                        new SelectorNode
                        (
                            new List<INode>()
                            {
                                new SequenceNode
                                (
                                    new List<INode>()
                                    {
                                        new ActionNode(CheckMoving),
                                        new ActionNode(MoveToDestination)
                                    }
                                ),
                                new ActionNode(WaitForMovement)
                            }
                        )
                    }
                )
            }
        );
    }

    #region BT_Recover
    INode.NodeState CheckRecovering()
    {
        if (_BTState == BTState.Recover)
            return INode.NodeState.Success;

        return INode.NodeState.Failure;
    }
    INode.NodeState WaitRecovering()
    {
        if (_recoverTimer < RecoverTime)
        {
            _recoverTimer += Time.deltaTime;

            if (RecoverTime - _recoverTimer < 1.5f && animator.GetBool(DIE_HASH))
                animator.SetBool(DIE_HASH, false);

            return INode.NodeState.Running;
        }
        _BTState = BTState.None;
        _recoverTimer = 0;


        return INode.NodeState.Success;
    }
    #endregion
    #region BT_Die
    INode.NodeState CheckDead()
    {
        if (_BTState == BTState.Die)
            return INode.NodeState.Success;

        return INode.NodeState.Failure;
    }

    #endregion
    #region BT_Hit
    INode.NodeState CheckHitting()
    {
        /*if (_isHit)
            return INode.NodeState.Success;*/

        return INode.NodeState.Failure;
    }
    INode.NodeState TakeHit()
    {
        if (_hitTimer < HitTime)
        {
            _hitTimer += Time.deltaTime;
            _agent.speed = 0;
            return INode.NodeState.Running;
        }
        //_isHit = false;
        _hitTimer = 0;
        _agent.speed = _stat.MoveSpeed;

        return INode.NodeState.Success;
    }
    #endregion
    #region BT_Talk
    INode.NodeState CheckTalking()
    {
        if (_BTState == BTState.Talk)
            return INode.NodeState.Success;

        return INode.NodeState.Failure;
    }
    INode.NodeState Talk()
    {
        _moveAnimTimer = Mathf.Max(0, _moveAnimTimer - Time.deltaTime * 2);
        animator.SetFloat(MOVE_HASH, _moveAnimTimer);
        _agent.isStopped = true;
        Rotation((Managers.PlayerInfo.Player.transform.position - transform.position).normalized);
        return INode.NodeState.Running;
    }
    #endregion
    #region BT_DetectEnemy
    INode.NodeState CheckDetectedEnemy()
    {
        if (_detection.DetectEnemyInDetectionRange())
            return INode.NodeState.Success;

        if (_BTState == BTState.Battle)
            _BTState = BTState.None;

        return INode.NodeState.Failure;
    }
    INode.NodeState CheckEnemyDistance()
    {
        _BTState = BTState.Battle;
        if (Vector3.Distance(transform.position, _detection.EnemyInDetectionRange.transform.position) < AttackDistance)
        {
            _agent.isStopped = true;
            return INode.NodeState.Success;
        }
        _moveAnimTimer = Mathf.Min(1, _moveAnimTimer + Time.deltaTime * 2);
        animator.SetFloat(MOVE_HASH, _moveAnimTimer);

        return INode.NodeState.Failure;
    }
    INode.NodeState StopAttack()
    {
        if (_attackTimer > 0)
        {
            _moveAnimTimer = Mathf.Max(0, _moveAnimTimer - Time.deltaTime * 2);
            animator.SetFloat(MOVE_HASH, _moveAnimTimer);
            _attackTimer -= Time.deltaTime;
            return INode.NodeState.Running;
        }

        return INode.NodeState.Success;
    }
    INode.NodeState Attacki()
    {
        _attackTimer = _stat.AttackSpeed;
        Rotation((_detection.EnemyInDetectionRange.transform.position - transform.position).normalized);
        animator.SetTrigger(ATTACK_HASH);

        return INode.NodeState.Success;
    }
    protected INode.NodeState StopMovingNearEnemy()
    {
        if (_agent.isStopped && _stopMovingNearEnemyTimer > 0)
        {
            _stopMovingNearEnemyTimer -= Time.deltaTime;
            return INode.NodeState.Running;
        }

        return INode.NodeState.Success;
    }
    INode.NodeState MoveNearEnemy()
    {
        _stopMovingNearEnemyTimer = 1;
        _agent.isStopped = false;
        _agent.speed = _stat.MoveSpeed * RunSpeed;
        _agent.SetDestination(_detection.EnemyInDetectionRange.transform.position);

        return INode.NodeState.Running;
    }
    #endregion
    #region BT_FollowPlayer
    INode.NodeState CheckFollowPlayer()
    {
        if (_BTState == BTState.FollowPlayer)
        {
            _agent.stoppingDistance = PlayerStoppingDistance;

            return INode.NodeState.Success;
        }
        _agent.stoppingDistance = 0;

        return INode.NodeState.Failure;
    }
    INode.NodeState StopFollowing()
    {
        if (Vector3.Distance(transform.position, Managers.PlayerInfo.Player.transform.position) > PlayerStoppingDistance)
        {
            return INode.NodeState.Failure;
        }
        else
        {
            _moveAnimTimer = Mathf.Max(0, _moveAnimTimer - Time.deltaTime * 2);
            animator.SetFloat(MOVE_HASH, _moveAnimTimer);
            _agent.ResetPath();
            Rotation((Managers.PlayerInfo.Player.transform.position - transform.position).normalized);

            return INode.NodeState.Running;
        }
    }
    INode.NodeState FollowPlayer()
    {
        if (_detection.DetectPlayer())
        {
            if (_moveAnimTimer > 1)
                _moveAnimTimer = Mathf.Max(0, _moveAnimTimer - Time.deltaTime * 2);
            else
                _moveAnimTimer = Mathf.Min(1, _moveAnimTimer + Time.deltaTime * 2);
            _agent.speed = _stat.MoveSpeed;
        }
        else
        {
            _moveAnimTimer = Mathf.Min(2, _moveAnimTimer + Time.deltaTime * 2);
            _agent.speed = _stat.MoveSpeed * RunSpeed;
        }
        animator.SetFloat(MOVE_HASH, _moveAnimTimer);
        _agent.SetDestination(Managers.PlayerInfo.Player.transform.position);

        return INode.NodeState.Running;
    }
    #endregion
    #region BT_Pattrol
    INode.NodeState CheckMoveToDestination()
    {
        if (PatrolDestinations != null)
            return INode.NodeState.Success;

        //rota
        return INode.NodeState.Failure;
    }
    INode.NodeState CheckMoving()
    {
        if (_BTState != BTState.MoveToDestination)
        {
            _agent.ResetPath();
            return INode.NodeState.Success;
        }
        else if (_BTState == BTState.MoveToDestination && Vector3.Distance(transform.position, _agent.destination) > 0.1f)
        {
            _moveAnimTimer = Mathf.Min(1, _moveAnimTimer + Time.deltaTime * 2);
            animator.SetFloat(MOVE_HASH, _moveAnimTimer);
            return INode.NodeState.Running;
        }
        return INode.NodeState.Failure;
    }
    INode.NodeState MoveToDestination()
    {
        _BTState = BTState.MoveToDestination;
        Vector3 dest = PatrolDestinations.GetPositionNearestDestination(transform.position);
        _agent.SetDestination(dest);
        _agent.speed = _stat.MoveSpeed;
        _agent.stoppingDistance = 0;

        return INode.NodeState.Success;
    }
    INode.NodeState WaitForMovement()
    {
        if (_stopMovingTimer < StopMovingTime)
        {
            _moveAnimTimer = Mathf.Max(0, _moveAnimTimer - Time.deltaTime * 2);
            animator.SetFloat(MOVE_HASH, _moveAnimTimer);
            _agent.ResetPath();
            _stopMovingTimer += Time.deltaTime;

            return INode.NodeState.Running;
        }
        _stopMovingTimer = 0;
        _BTState = BTState.None;

        return INode.NodeState.Failure;
    }
    #endregion
}
