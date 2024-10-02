using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public abstract class BaseDetection : MonoBehaviour
{
    const int NON_ALLOC_SIZE = 5;

    protected int enemyMask;
    protected Collider[] detectedEnemyInDetectionRange = new Collider[NON_ALLOC_SIZE];
    protected Collider[] detectedEnemysInAttackRange = new Collider[NON_ALLOC_SIZE];
    Dictionary<int, BaseController> _detectionControllerPool = new Dictionary<int, BaseController>();
    Dictionary<int, BaseController> _attackControllerPool = new Dictionary<int, BaseController>();

    [SerializeField]
    float _detectionRange;
    [SerializeField]
    float _detectionViewAngle;
    [SerializeField]
    float _attackRange;
    [SerializeField]
    float _attackDistance;

    public float DetectionRange => _detectionRange;
    public float DetectionViewAngle => _detectionViewAngle;

    public Rigidbody EnemyInDetectionRange { get; protected set; }
    public Collider EnemyInAttackRange { get; protected set; }

    private void OnEnable()
    {
        Init();
    }

    protected abstract bool Init();

    //감지 범위, 각도 변경
    public void ChangeDetectionRangeOption(float detectionRange, float detectionViewAngle)
    {
        this._detectionRange = detectionRange;
        this._detectionViewAngle = detectionViewAngle;
    }
    //공격 범위 변경
    public void ChangeAttackRange(float attackRange, float attackDistance)
    {
        this._attackRange = attackRange;
        this._attackDistance = attackDistance;
    }
    //감지 범위 내의 가장 가까운 적 반환
    public bool DetectEnemyInDetectionRange()
    {
        Rigidbody target = null;
        float min = float.MaxValue;
        int count = Physics.OverlapSphereNonAlloc(transform.position, _detectionRange, detectedEnemyInDetectionRange, enemyMask);
        for (int i = 0; i < count; i++)
        {
            var rb = detectedEnemyInDetectionRange[i].attachedRigidbody;
            int instanceID = rb.GetInstanceID();
            if (_detectionControllerPool.ContainsKey(instanceID) == false)
            {
                var controller = rb.GetComponent<BaseController>();
                _detectionControllerPool.Add(instanceID, controller);
            }

            //시아각 검사
            Vector3 targetDir = (rb.transform.position - transform.position).normalized;
            float targetAngle = Mathf.Acos(Vector3.Dot(transform.forward, targetDir)) * Mathf.Rad2Deg;
            var dist = Vector3.Distance(transform.position, rb.transform.position);

            if (targetAngle <= _detectionViewAngle * 0.5f)
            {
                //벽 검사
                Ray ray = new Ray(transform.position + Vector3.up, targetDir);
                if (Physics.Raycast(ray, dist, Managers.Layer.GroundLayerMask) == false)
                {
                    //거리검사
                    if (dist < min)
                    {
                        var controller = _detectionControllerPool[instanceID];
                        if (controller != null && controller.IsDead == false)
                        {
                            min = dist;
                            target = rb;
                            gizmoRay = ray;
                            gizmoDist = dist;
                        }
                    }
                }
            }
            if (dist > _detectionRange)
                _detectionControllerPool.Remove(instanceID);
        }
        EnemyInDetectionRange = target;
        return target != null;
    }
    Ray gizmoRay;
    float gizmoDist;
    //공격 범위 내의 가까운 적 감지
    public bool DetectEnemyInAttackRange()
    {
        Collider target = null;
        float min = float.MaxValue;
        int count = Physics.OverlapSphereNonAlloc(transform.position + transform.forward * _attackDistance, _attackRange, detectedEnemysInAttackRange, enemyMask);
        for (int i = 0; i < count; i++)
        {
            var rb = detectedEnemysInAttackRange[i].attachedRigidbody;
            int instanceID = rb.GetInstanceID();
            if (_attackControllerPool.ContainsKey(instanceID) == false)
            {
                var controller = rb.GetComponent<BaseController>();
                _attackControllerPool.Add(instanceID, controller);
            }

            var dist = Vector3.Distance(transform.position, rb.transform.position);
            if (dist < min)
            {
                var controller = _attackControllerPool[instanceID];
                if (controller != null && controller.IsDead == false)
                {
                    min = dist;
                    target = detectedEnemysInAttackRange[i];
                }
            }

            if (dist > _detectionRange)
                _attackControllerPool.Remove(instanceID);
        }
        EnemyInAttackRange = target;
        return target != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * _attackDistance, _attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(gizmoRay.origin, gizmoRay.direction * gizmoDist);
    }
}

