using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerInfoManager;

public class LavaField : MonoBehaviour
{
    const string HIT_EFFECT = "Skill/FirebreathHitEffect";
    const string MONSTER = "Monster/Slime";
    const float INTERVAL_SPAWN_TIME = 4;
    const float INTERVAL_DOT_DAMAGE_TIME = 1;

    ParticleSystem _particleSystem;
    Collider[] _playerColliders = new Collider[1];
    float _durationTime;
    float _durationTimer;
    float _spawnTimer;
    float _detectRadius;
    float _dotDamageTimer;
    int _damage;

    private void Start()
    {
        Init(5);
    }

    public void Init(int damage)
    {
        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();

        _damage = damage;
        _durationTime = _particleSystem.main.startLifetime.constant;
        _durationTimer = _durationTime;
        _spawnTimer = _durationTimer - INTERVAL_SPAWN_TIME;
        _dotDamageTimer = INTERVAL_DOT_DAMAGE_TIME;
    }

    private void Update()
    {
        _durationTimer -= Time.deltaTime;

        AdjustRadiusOverDurationTime();
        SpawnPerIntervalTime();
        InflictDamagePerIntervalTime();
        DestroyOnDurationEnd();
    }

    void AdjustRadiusOverDurationTime()
    {
        if (_durationTimer > 12)
        {
            _detectRadius = 7 + (_durationTime - _durationTimer) * 0.9f;
        }
        else if (_durationTimer < 3)
        {
            _detectRadius -= Time.deltaTime * 2;
        }
    }

    void SpawnPerIntervalTime()
    {
        if (_durationTimer < _spawnTimer)
        {
            _spawnTimer -= INTERVAL_SPAWN_TIME;
            var slime = Managers.Resource.Instantiate(MONSTER);
            float radius = _detectRadius / 2;
            float randomPosX = Random.Range(transform.position.x - radius, transform.position.x + radius);
            float randomPosZ = Random.Range(transform.position.z - radius, transform.position.z + radius);

            slime.GetComponent<MonsterController>().InitSpawnSetting(new Vector3(randomPosX, 0, randomPosZ));
        }
    }

    void InflictDamagePerIntervalTime()
    {
        if (_dotDamageTimer > 0)
            _dotDamageTimer -= Time.deltaTime;
        else
        {
            if (Physics.OverlapSphereNonAlloc(transform.position, _detectRadius, _playerColliders, (1 << Managers.Layer.PlayerLayer)) > 0)
            {
                _dotDamageTimer = INTERVAL_DOT_DAMAGE_TIME;
                HitTarget(_playerColliders[0], HIT_EFFECT);
            }
        }
    }

    void DestroyOnDurationEnd()
    {
        if (_durationTimer < 0)
            Managers.Resource.Destroy(gameObject);
    }

    protected bool HitTarget(Collider collider, string hitEffectName = null)
    {
        IDamageable iDamage = Util.FindParent<IDamageable>(collider.gameObject);
        if (iDamage == null)
            return false;

        Damage damage = iDamage.TakeDamage(new Damage() { Value = _damage });

        var hitPoint = collider.ClosestPoint(transform.position);
        Managers.InfoUI.ShowDamageEffect(hitPoint, damage);

        if (hitEffectName != null)
        {
            var hitEffect = Managers.Resource.Instantiate(hitEffectName);
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(transform.forward);
        }
        return true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _detectRadius);
    }
}
