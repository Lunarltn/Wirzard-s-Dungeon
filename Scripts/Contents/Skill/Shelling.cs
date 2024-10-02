using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Shelling : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "ShellHitEffect";
    const string CHARGE_EFFECT = SUB_EFFECT_PATH + "ShellChargeEffect";
    const float BOOM_RANGE = 2.5f;
    const float CHARGE_TIME = 5;
    const float INIT_SCALE = 0.6f;
    const int CHARGE_STEP = 2;

    float _chargeTimer;
    float _shotTime;
    bool _isShot;
    int _chargeStep;

    GameObject _chargeEffect;

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);

        currentDuration += CHARGE_TIME;
        if (_chargeEffect == null)
            _chargeEffect = Managers.Resource.Instantiate(CHARGE_EFFECT);
        _isShot = false;
        _chargeStep = CHARGE_STEP;
        _chargeTimer = 0;
        _shotTime = 0;
        _chargeEffect.SetActive(true);
        for (int i = 0; i < 2; i++)
        {
            _chargeEffect.transform.GetChild(i).gameObject.SetActive(true);
            var emission = _chargeEffect.transform.GetChild(i).GetComponent<ParticleSystem>().emission;
            emission.rateOverTime = _chargeStep;

            transform.GetChild(i).gameObject.SetActive(false);
        }
        _chargeEffect.transform.GetChild(2).gameObject.SetActive(false);

        InstantiateScorched("ShellingScorched");
    }

    protected override void UpdateSkill()
    {
        _chargeTimer += Time.deltaTime;

        Vector3 attackDir = (Managers.Camera.DetectAimPointRayPosition() - skillInfo.casterParent.position).normalized;

        if (_isShot == false)
        {
            _chargeEffect.transform.position = skillInfo.casterParent.position;
            _chargeEffect.transform.rotation = Quaternion.LookRotation(attackDir);

            transform.position = skillInfo.casterParent.position;
            transform.rotation = Quaternion.LookRotation(attackDir);
            transform.localScale = Vector3.one * INIT_SCALE + Vector3.one * ((_chargeTimer / CHARGE_TIME) * (1 - INIT_SCALE));
        }

        if (_chargeTimer > _chargeStep)
        {
            for (int i = 0; i < 2; i++)
            {
                var emission = _chargeEffect.transform.GetChild(i).GetComponent<ParticleSystem>().emission;
                emission.rateOverTime = _chargeStep * 2;
            }
            _chargeStep++;
        }

        if ((CHARGE_TIME < _chargeTimer || Managers.Input.MouseLeftClick == false) && _isShot == false)
        {
            _isShot = true;
            _shotTime = _chargeTimer;
            _chargeEffect.transform.GetChild(0).gameObject.SetActive(false);
            _chargeEffect.transform.GetChild(1).gameObject.SetActive(false);
            _chargeEffect.transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(1).gameObject.SetActive(true);

            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().AddForce(attackDir * skillInfo.speed, ForceMode.Impulse);
        }

        if (_isShot && _chargeTimer - _shotTime < 0.2f)
        {
            Collider[] colliders = Physics.OverlapSphere(skillInfo.casterParent.position, 3, LayerMask.GetMask("Player"));
            if (colliders.Length > 0)
                colliders[0].GetComponent<Rigidbody>().AddForce(-attackDir * _chargeStep * 0.8f, ForceMode.Impulse);
        }

    }

    void OnCollisionEnter(Collision other)
    {
        var layer = other.gameObject.layer;
        if (CheckExcludedLayers(layer))
            return;
        if (_isShot == false)
            return;

        DestroyDestructibleObject(15);

        Collider[] colliders = Physics.OverlapSphere(transform.position, BOOM_RANGE, (1 << skillTargetLayer));
        for (int i = 0; i < colliders.Length; i++)
            HitTarget(colliders[i]);

        if (CheckScorchedLayers(layer))
            EmitScorched(other.contacts[0].point, other.contacts[0].normal, 5, 3);

        Managers.Camera.PlayShake(10, 0.8f);
        CreateHitEffect(other, HIT_EFFECT);
        Stop();
        Managers.Resource.Destroy(gameObject);
    }

    public override void Stop()
    {
        _chargeEffect.SetActive(false);
    }
}