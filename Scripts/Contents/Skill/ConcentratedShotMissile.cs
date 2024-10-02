using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;

public class ConcentratedShotMissile : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "MissileHitEffect";

    ParticleSystem _particleSystem;
    ParticleSystemRenderer _renderer;
    MaterialPropertyBlock _mpb;
    float _startPointWeight = 6.0f;
    float _endPointWeight = 3.0f;
    float _curveTimer;
    Vector3[] _points = new Vector3[4];
    bool _isDestroy;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _renderer = _particleSystem.gameObject.GetComponent<ParticleSystemRenderer>();
        if (_mpb == null) _mpb = new MaterialPropertyBlock();
        base.Init(skillInfo, skillTarget);
        transform.position = skillInfo.casterParent.position;
        transform.rotation = Quaternion.LookRotation((skillInfo.moveTargetPos() - skillInfo.casterParent.position).normalized);
        InitPoints();
        _curveTimer = 0;
        _isDestroy = false;
    }

    protected override void UpdateSkill()
    {
        _curveTimer += skillInfo.speed * 0.2f * Time.deltaTime;
        transform.position = new Vector3(
            CubicBezierCurve(_points[0].x, _points[1].x, _points[2].x, _points[3].x),
            CubicBezierCurve(_points[0].y, _points[1].y, _points[2].y, _points[3].y),
            CubicBezierCurve(_points[0].z, _points[1].z, _points[2].z, _points[3].z));
    }

    void InitPoints()
    {

        _points[0] = transform.position;
        _points[1] = transform.position +
            (_startPointWeight * Random.Range(-1.0f, 1.0f) * transform.right) +
            (_startPointWeight * Random.Range(-0.15f, 1.0f) * transform.up) +
            (_startPointWeight * Random.Range(-1.0f, -0.8f) * transform.forward);
        _points[2] = skillInfo.moveTargetPos() +
            (_endPointWeight * Random.Range(-1.0f, 1.0f) * transform.right) +
            (_endPointWeight * Random.Range(-1.0f, 1.0f) * transform.up) +
            (_endPointWeight * Random.Range(0.8f, 1.0f) * transform.forward);
        _points[3] = skillInfo.moveTargetPos();
    }

    private float CubicBezierCurve(float a, float b, float c, float d)
    {
        float t = _curveTimer / skillInfo.duration;
        float ab = Mathf.Lerp(a, b, t);
        float bc = Mathf.Lerp(b, c, t);
        float cd = Mathf.Lerp(c, d, t);

        float abbc = Mathf.Lerp(ab, bc, t);
        float bccd = Mathf.Lerp(bc, cd, t);

        return Mathf.Lerp(abbc, bccd, t);
    }

    public void ChangeRimcolorStrength(float strength)
    {
        _renderer.GetPropertyBlock(_mpb);
        _mpb.SetFloat("_RimStrength", strength);
        _renderer.SetPropertyBlock(_mpb);
    }

    public void ChangeSize(float size)
    {
        var main = _particleSystem.main;
        main.startSize = size;
    }


    private void OnTriggerEnter(Collider other)
    {
        var layer = other.gameObject.layer;
        if (CheckExcludedLayers(layer))
            return;

        if (layer == skillTargetLayer)
        {
            HitTarget(other);
            CreateHitEffect(other, HIT_EFFECT);
        }

        if (_isDestroy == false)
        {
            _isDestroy = true;
            currentDuration = 1.0f;
        }
    }

}