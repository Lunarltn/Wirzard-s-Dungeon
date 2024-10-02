using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameInjection : SkillCast
{
    const string HIT_EFFECT = "HitEffect";
    const float MAX_DISTANCSE = 20;
    const float TICK_DAMAGE_TIME = 0.5f;

    float _tickDamageTimer;

    LineRenderer _lineRenderer;
    GameObject _hitEffect;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        if (_lineRenderer == null)
            _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _hitEffect = Util.FindChild(transform.gameObject, HIT_EFFECT, true);
        _hitEffect.SetActive(false);
        _tickDamageTimer = 0;

        InstantiateScorched("FlameInjectionScorched");
    }

    protected override void UpdateSkill()
    {
        transform.position = skillInfo.casterParent.position;

        if (Managers.Input.MouseLeftClick == false)
        {
            Stop();
        }
        else
        {
            Managers.Camera.SetMaxAimDistance(MAX_DISTANCSE);
            _lineRenderer.positionCount = 2;
            _lineRenderer.SetPosition(0, transform.position);
            _lineRenderer.SetPosition(1, skillInfo.moveTargetPos.Invoke());
        }

        if (Managers.Camera.HitAimPointRay.transform != null)
        {
            _hitEffect.SetActive(true);
            _hitEffect.transform.position = Managers.Camera.HitAimPointRay.point;
            _hitEffect.transform.rotation = Quaternion.LookRotation(Managers.Camera.HitAimPointRay.normal);

            int layer = Managers.Camera.HitAimPointRay.transform.gameObject.layer;

            if (CheckScorchedLayers(layer))
                EmitScorched(Managers.Camera.HitAimPointRay.point, Managers.Camera.HitAimPointRay.normal, 5, 0.5f);

            _tickDamageTimer += Time.deltaTime;
            if (_tickDamageTimer > TICK_DAMAGE_TIME)
            {
                _tickDamageTimer = 0;

                if (layer == skillTargetLayer)
                    HitTarget(Managers.Camera.HitAimPointRay);
            }
        }
        else
        {
            _hitEffect.SetActive(false);
        }
    }

    public override void Stop()
    {
        Managers.Camera.SetMaxAimDistance();
        _lineRenderer.positionCount = 0;
        Managers.Resource.Destroy(gameObject);
    }
}
