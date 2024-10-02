using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class BossColliderController : MonoBehaviour, IDamageable
{
    const string MONSTER = "Monster";
    const string HEAD = "Head";
    Collider _collider;
    Func<bool, Damage, Damage> _takeDamage;
    bool _isHead;

    public Collider Collider => _collider;

    public void Init(Func<bool, Damage, Damage> takeDamage)
    {
        transform.tag = MONSTER;
        _collider = GetComponent<Collider>();
        _isHead = string.Compare(transform.name, HEAD) == 0;
        _takeDamage = takeDamage;
    }

    public Damage TakeDamage(Damage damage)
    {
        return _takeDamage(_isHead, damage);
    }
}
