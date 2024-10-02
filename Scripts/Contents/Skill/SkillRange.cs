using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SkillRange : MonoBehaviour
{
    ParticleSystem _particleSystem;

    public void Init(float size, float time)
    {
        if (_particleSystem == null)
            _particleSystem = GetComponent<ParticleSystem>();

        var main = _particleSystem.main;
        main.startLifetime = time;
        main.startSize = size;
    }
}
