using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GolemStoneBall : SkillCast
{
    const string SMOKE = SUB_EFFECT_PATH + "Smoke";
    const float DESTROY_TIME = 5;
    const float EXPLOSION_FORCE = 10;
    const float SKILL_RANGE = 3;

    MeshRenderer _meshRenderer;
    Collider _collider;
    Transform _child;
    List<Vector3> _fragmentPositions = new List<Vector3>();
    List<Rigidbody> _debris = new List<Rigidbody>();
    float _timer;
    bool _isInit;
    bool _isTrowing;
    Vector3[] _points = new Vector3[3];

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        if (_isInit == false)
        {
            _isInit = true;
            _meshRenderer = GetComponent<MeshRenderer>();
            _collider = GetComponent<Collider>();
            _child = transform.GetChild(0);
            for (int i = 0; i < _child.childCount; i++)
            {
                _fragmentPositions.Add(_child.GetChild(i).localPosition);
                _debris.Add(_child.GetChild(i).GetComponent<Rigidbody>());
            }
        }

        base.Init(skillInfo, skillTarget);
        transform.parent = skillInfo.casterParent;
        transform.localPosition = Vector3.zero;

        _timer = 0;
        _isTrowing = false;
        _meshRenderer.enabled = true;
        _child.gameObject.SetActive(false);
        _collider.enabled = true;
        for (int i = 0; i < _fragmentPositions.Count; i++)
        {
            _child.GetChild(i).rotation = Quaternion.identity;
            _child.GetChild(i).localScale = Vector3.one;
            _child.GetChild(i).localPosition = _fragmentPositions[i];
        }
    }

    protected override void UpdateSkill()
    {
        if (_isTrowing)
        {
            if (_timer < 1)
            {
                _timer += Time.deltaTime * skillInfo.speed;
                transform.position = CubicBezierCurve(_points[0], _points[1], _points[2], _timer);
            }
        }

        if (currentDuration < 1)
        {
            for (int i = 0; i < _child.childCount; i++)
                _child.GetChild(i).localScale = Vector3.one * currentDuration;
        }
    }

    public void Thorw()
    {
        _points[0] = transform.position;
        if (skillInfo.targetCollider != null)
            _points[2] = skillInfo.targetCollider.transform.position;
        else
            _points[2] = skillInfo.stopTargetPos;
        var dir = (_points[2] - _points[0]);
        _points[1] = _points[0] + dir / 2 + Vector3.up * 5;

        transform.parent = null;
        _isTrowing = true;

        CreateSkillRange(GetFloorHeight(_points[2]), SKILL_RANGE, 1);
    }

    private void OnTriggerEnter(Collider other)
    {

        if (_isTrowing == false || skillCasterLayer == other.gameObject.layer)
            return;

        Collider[] colliders = Physics.OverlapSphere(transform.position, SKILL_RANGE, skillTargetLayerMask);
        for (int i = 0; i < colliders.Length; i++)
        {
            HitTarget(colliders[i]);
            var force = (colliders[i].ClosestPoint(transform.position) - transform.position).normalized * 20;
            colliders[i].transform.GetComponent<IKnockbackable>().GetKnockedBack(force);
        }

        currentDuration = DESTROY_TIME;
        _meshRenderer.enabled = false;
        _child.gameObject.SetActive(true);
        _collider.enabled = false;
        Managers.Camera.PlayShake(5, 0.5f);
        AddExplosionForceAtDebris(EXPLOSION_FORCE, transform.position);
        CreateHitEffect(SMOKE, true);
    }

    private Vector3 CubicBezierCurve(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        var ab = Vector3.Lerp(a, b, t);
        var bc = Vector3.Lerp(b, c, t);

        return Vector3.Lerp(ab, bc, t);
    }

    void AddExplosionForceAtDebris(float force, Vector3 explosionPosition)
    {
        foreach (Rigidbody rigidbody in _debris)
        {
            rigidbody.AddExplosionForce(force, explosionPosition, 6, 0, ForceMode.Impulse);
        }
    }
    private void OnDrawGizmos()
    {
        if (_points[0] == Vector3.zero)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_points[0], _points[1]);
        Gizmos.DrawLine(_points[1], _points[2]);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_points[2], SKILL_RANGE);
    }
}
