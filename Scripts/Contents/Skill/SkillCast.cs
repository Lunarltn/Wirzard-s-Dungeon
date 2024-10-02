using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public abstract class SkillCast : MonoBehaviour
{
    protected const string SKILL_PATH = "Skill/";
    protected const string SUB_EFFECT_PATH = "Skill/SubEffect/";
    const string SKILL_RANGE_PATH = "Skill/SkillRange";
    protected const string SCORCHED = "Skill/Scorched";
    protected const string GORUND = "Ground";
    protected const string OBJECT = "Object";
    protected const string SKILL = "Skill";
    protected SkillInfo skillInfo;
    protected float currentDuration;
    protected int skillTargetLayer;
    protected int skillCasterLayer;
    protected int excludedLayerMask;
    protected int skillTargetLayerMask;
    protected int skillCasterLayerMask;

    protected int scorchLayerMask;
    protected Define.SkillTarget skillTarget;
    Scorched _scorched;

    public virtual void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        this.skillInfo = skillInfo;
        currentDuration = skillInfo.duration;
        this.skillTarget = skillTarget;
        string skillTargetTag = Enum.GetName(typeof(Define.SkillTarget), skillTarget);
        skillTargetLayer = LayerMask.NameToLayer(skillTargetTag);
        skillTargetLayerMask = (1 << skillTargetLayer);
        string skillCasterTag = Enum.GetName(typeof(Define.SkillTarget), (Define.SkillTarget)((int)skillTarget == 0 ? 1 : 0));
        skillCasterLayer = LayerMask.NameToLayer(skillCasterTag);
        skillCasterLayerMask = (1 << skillCasterLayer);
        excludedLayerMask = (1 << skillCasterLayer) | Managers.Layer.SkillLayerMask | 1;

        scorchLayerMask = (1 << Managers.Layer.GroundLayer) | (1 << Managers.Layer.ObjectLayer);
    }

    private void FixedUpdate()
    {
        if (currentDuration > 0)
        {
            currentDuration -= Time.deltaTime;
            UpdateSkill();
        }
        else
        {
            Managers.Resource.Destroy(gameObject);
        }
    }

    protected abstract void UpdateSkill();

    void OnDisable()
    {
        Disable();
    }

    protected virtual void Disable()
    {
        currentDuration = 0;
    }

    protected void InstantiateScorched(string name)
    {
        if (_scorched != null)
            return;

        _scorched = Managers.Resource.Instantiate(SCORCHED).GetComponent<Scorched>();
        _scorched.gameObject.name = name;
    }

    protected void EmitScorched(Vector3 point, Vector3 normal, int count, float size)
    {
        _scorched.Emit(point, normal, count, size);
    }

    public SkillInfo GetSkillInfo() { return skillInfo; }

    protected bool DestroyDestructibleObject(float force)
    {
        var destructibleObjects = Physics.OverlapSphere(transform.position, 2, LayerMask.GetMask("DestructibleObject"));
        for (int i = 0; i < destructibleObjects.Length; i++)
        {
            DestructibleObject destObject = destructibleObjects[i].GetComponent<DestructibleObject>();
            if (destObject != null)
                destObject.Destroy(force, transform.position);
            else
                destructibleObjects[i].GetComponent<Rigidbody>()?.AddExplosionForce(force, transform.position, 5, 2, ForceMode.Impulse);
        }
        return destructibleObjects.Length > 0;
    }

    protected bool CheckExcludedLayers(int layer)
    {
        return (layer == skillCasterLayer || layer == Managers.Layer.SkillLayer);
    }

    protected bool CheckScorchedLayers(int layer)
    {
        return (layer == Managers.Layer.ObjectLayer || layer == Managers.Layer.GroundLayer);
    }

    protected bool HitTarget(RaycastHit hit)
    {
        IDamageable iDamage = Util.FindParent<IDamageable>(hit.transform.gameObject);
        if (iDamage == null)
            return false;

        Damage damage = new Damage()
        {
            Value = skillInfo.damage,
            Caster = skillInfo.casterTransform
        };
        Damage fanalDamage = iDamage.TakeDamage(damage);
        Managers.InfoUI.ShowDamageEffect(hit.point, fanalDamage);

        return true;
    }
    protected GameObject CreateHitEffect(string hitEffectName, bool isGround = false)
    {
        var hitEffect = Managers.Resource.Instantiate(hitEffectName);
        var position = isGround ? GetFloorHeight(transform.position) : transform.position;
        hitEffect.transform.position = position;
        return hitEffect;
    }

    protected GameObject CreateHitEffect(RaycastHit hit, string hitEffectName)
    {
        var hitEffect = Managers.Resource.Instantiate(hitEffectName);
        hitEffect.transform.position = hit.point;
        hitEffect.transform.rotation = Quaternion.LookRotation(transform.forward);
        return hitEffect;
    }

    protected GameObject CreateHitEffect(Collision other, string hitEffectName)
    {
        var hitEffect = Managers.Resource.Instantiate(hitEffectName);
        hitEffect.transform.position = other.contacts[0].point;
        hitEffect.transform.rotation = Quaternion.LookRotation(transform.forward);
        return hitEffect;
    }


    protected bool HitTarget(Collider collider)
    {
        IDamageable iDamage = Util.FindParent<IDamageable>(collider.gameObject);
        if (iDamage == null)
            return false;

        Damage damage = new Damage()
        {
            Value = skillInfo.damage,
            Caster = skillInfo.casterTransform
        };
        Damage fanalDamage = iDamage.TakeDamage(damage);
        var hitPoint = collider.ClosestPoint(transform.position);
        Managers.InfoUI.ShowDamageEffect(hitPoint, fanalDamage);

        return true;
    }

    protected GameObject CreateHitEffect(Collider collider, string hitEffectPath)
    {
        var hitEffect = Managers.Resource.Instantiate(hitEffectPath);
        if (hitEffect == null)
            return null;
        var hitPoint = collider.ClosestPoint(transform.position);
        hitEffect.transform.position = hitPoint;
        hitEffect.transform.rotation = Quaternion.LookRotation(transform.forward);
        return hitEffect;
    }

    protected void CreateSkillRange(Vector3 position, float size, float time)
    {
        var skillRange = Managers.Resource.Instantiate(SKILL_RANGE_PATH);
        skillRange.transform.position = position;
        skillRange.GetComponent<SkillRange>().Init(size * 0.09f, time);
    }

    protected Vector3 GetFloorHeight(Vector3 position)
    {
        float height = 0;
        if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out RaycastHit hit, 100, Managers.Layer.GroundLayerMask))
            height = hit.point.y;
        position.y = height;

        return position;
    }

    public float GetRandomValue(float value)
    {
        return UnityEngine.Random.Range(-value, value);
    }

    public float GetRandomValue2(float value)
    {
        return UnityEngine.Random.Range(0, value);
    }

    public Vector3 GetRandomVector(Vector3 value)
    {
        Vector3 result;
        result.x = GetRandomValue(value.x);
        result.y = GetRandomValue(value.y);
        result.z = GetRandomValue(value.z);
        return result;
    }

    public Vector3 GetRandomVector2(Vector3 value)
    {
        Vector3 result;
        result.x = GetRandomValue2(value.x);
        result.y = GetRandomValue2(value.y);
        result.z = GetRandomValue2(value.z);
        return result;
    }

    public virtual void Stop() { }
}
