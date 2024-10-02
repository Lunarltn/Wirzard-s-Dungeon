using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : SkillCast
{
    const string HIT_EFFECT = SUB_EFFECT_PATH + "FireballHitEffect";
    const float BOOM_RANGE = 3;
    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);

        Vector3 _attackDir = (skillInfo.stopTargetPos - skillInfo.casterParent.position).normalized;

        transform.position = skillInfo.casterParent.position;
        transform.rotation = Quaternion.LookRotation(_attackDir);
        InstantiateScorched("FireballScorched");
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        GetComponent<Rigidbody>().angularVelocity = transform.forward * 100;

        GetComponent<Rigidbody>().AddForce(_attackDir * skillInfo.speed, ForceMode.Impulse);
    }

    protected override void UpdateSkill()
    {
    }

    private void OnCollisionEnter(Collision collision)
    {
        var layer = collision.gameObject.layer;

        if (CheckExcludedLayers(layer))
            return;

        DestroyDestructibleObject(15);

        Collider[] colliders = Physics.OverlapSphere(transform.position, BOOM_RANGE, (1 << skillTargetLayer));
        for (int i = 0; i < colliders.Length; i++)
            HitTarget(colliders[i]);

        if (CheckScorchedLayers(layer))
            EmitScorched(collision.contacts[0].point, collision.contacts[0].normal, 5, 3);

        Managers.Camera.PlayShake(5, 0.5f);
        GameObject hitEffect = Managers.Resource.Instantiate(HIT_EFFECT);
        hitEffect.transform.position = transform.position;
        hitEffect.transform.rotation = Quaternion.LookRotation(-transform.forward);
        Managers.Resource.Destroy(gameObject);
    }
}
