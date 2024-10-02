using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class DestroyEffect : MonoBehaviour
{
    [SerializeField]
    float _durationTime;
    float _durationTimer;
    ParticleSystem _particleSystem;
    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _durationTimer = _durationTime;
    }

    private void Update()
    {
        if (_particleSystem == null)
        {
            _durationTimer -= Time.deltaTime;
            if (_durationTimer < 0)
                Managers.Resource.Destroy(gameObject);
        }
        else if (_particleSystem.isPlaying == false)
            Managers.Resource.Destroy(gameObject);
    }

}
