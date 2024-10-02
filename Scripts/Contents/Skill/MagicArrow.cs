using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicArrow : SkillCast
{
    const string PART_PATH = "Skill/MagicArrowProjectile";
    const float RANGE = 15f;
    const float INTERVAL_TIME = 0.2f;
    const int ARRAY_RANGE = 10;

    [SerializeField]
    ParticleSystem[] effects = new ParticleSystem[2];
    [SerializeField]
    Transform ringEffect;
    float[] ringEffectPosZ;

    float _intervalTimer;
    Collider[] _targets;

    private void Awake()
    {
        _targets = new Collider[ARRAY_RANGE];
        ringEffectPosZ = new float[ringEffect.childCount];
        for (int i = 0; i < ringEffect.childCount; i++)
            ringEffectPosZ[i] = ringEffect.GetChild(i).localPosition.z;
    }

    public override void Init(SkillInfo skillInfo, Define.SkillTarget skillTarget)
    {
        base.Init(skillInfo, skillTarget);
        transform.position = skillInfo.casterParent.position + Vector3.up * 1;
        transform.rotation = Quaternion.LookRotation(skillInfo.stopTargetPos - transform.position);

        for (int i = 0; i < ringEffect.childCount; i++)
            ringEffect.GetChild(i).localPosition = Vector3.zero;
    }

    protected override void UpdateSkill()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, RANGE, _targets, skillTargetLayerMask);
        if (count == 0)
        {
            for (int i = 0; i < ringEffect.childCount; i++)
            {
                var z = Mathf.Max(ringEffect.GetChild(i).localPosition.z - Time.deltaTime, 0);
                ringEffect.GetChild(i).localPosition = new Vector3(0, 0, z);
            }
            return;
        }
        else
        {
            for (int i = 0; i < ringEffect.childCount; i++)
            {
                var z = Mathf.Min(ringEffect.GetChild(i).localPosition.z + Time.deltaTime * 2, ringEffectPosZ[i]);
                ringEffect.GetChild(i).localPosition = new Vector3(0, 0, z);
            }

            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].Emit(1);
            }
        }


        float min = float.MaxValue;
        Vector3 targetPos = Vector3.zero;
        for (int i = 0; i < count; i++)
        {
            var targetDir = (_targets[i].transform.position - transform.position).normalized;
            Ray ray = new Ray(transform.position, targetDir);
            var dist = Vector3.Distance(transform.position, _targets[i].transform.position);
            if (Physics.Raycast(ray, dist, skillTargetLayerMask))
            {
                if (dist < min)
                {
                    min = dist;
                    targetPos = _targets[i].transform.position;
                }
            }
        }
        var quat = Quaternion.LookRotation((targetPos - transform.position).normalized);
        transform.rotation = Quaternion.Lerp(transform.rotation, quat, Time.deltaTime * 3);

        if (_intervalTimer > 0)
        {
            _intervalTimer -= Time.deltaTime;
            return;
        }
        GameObject skill = Managers.Resource.Instantiate(PART_PATH);
        SkillInfo skillInfo = this.skillInfo;
        skillInfo.casterParent = transform;
        skill.GetComponent<MagicArrowProjectile>().Init(skillInfo, skillTarget);

        _intervalTimer = INTERVAL_TIME;
    }
}