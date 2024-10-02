using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scorched : MonoBehaviour
{
    ParticleSystem _particleSystem;
    float _extinctionTimer = 0;

    void Update()
    {
        _extinctionTimer -= Time.deltaTime;

        if (_extinctionTimer < 0)
        {
            gameObject.SetActive(false);
        }
    }

    public void Emit(Vector3 position, Vector3 normal, int count, float size)
    {
        if (gameObject.activeSelf == false)
            gameObject.SetActive(true);

        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();

        transform.position = position;

        _extinctionTimer = _particleSystem.main.startLifetime.constant;

        var particleParam = new ParticleSystem.EmitParams();
        particleParam.startSize = size;
        particleParam.rotation3D = Quaternion.LookRotation(normal).eulerAngles;
        _particleSystem.Emit(particleParam, count);
    }
}
