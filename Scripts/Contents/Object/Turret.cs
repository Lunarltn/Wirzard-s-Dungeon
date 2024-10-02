using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour, IDamageable
{
    [SerializeField]
    Transform _horizontalAxis;
    [SerializeField]
    Transform _verticalAxis;
    [SerializeField]
    Transform _muzzle;
    [SerializeField]
    ParticleSystem _shotEffect;


    const float DETECTION_RANGE = 40;
    const float ACTIVE_TIME = 10;
    int _bossLayerMask;
    Collider[] _collider = new Collider[1];
    bool _isActive;
    float _activeTimer;

    public Damage TakeDamage(Damage damage)
    {
        _isActive = true;

        return new Damage();
    }

    void Start()
    {
        _bossLayerMask = LayerMask.GetMask("Monster");
    }

    void Update()
    {
        if (_isActive)
        {
            if (_activeTimer < ACTIVE_TIME)
                _activeTimer += Time.deltaTime;
            else
            {
                _activeTimer = 0;
                _isActive = false;
            }
            DetectMonster();
        }
    }

    void DetectMonster()
    {
        int length = Physics.OverlapSphereNonAlloc(transform.position, DETECTION_RANGE, _collider, _bossLayerMask);
        if (length > 0)
        {

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, DETECTION_RANGE);
    }
}
