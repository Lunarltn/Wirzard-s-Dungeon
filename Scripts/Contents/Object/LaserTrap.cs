using RayFire;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    GameObject _hitEffect;

    private void Start()
    {
        _hitEffect = transform.GetChild(0).gameObject;
        _hitEffect.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Managers.Layer.PlayerLayer)
        {
            HitTarget(collision);
            HitEffect(collision);
        }
    }

    protected bool HitTarget(Collision collision)
    {
        IDamageable iDamage = Util.FindParent<IDamageable>(collision.gameObject);
        if (iDamage == null)
            return false;

        Damage damage = iDamage.TakeDamage(new Damage() { Value = 10 });

        var hitPoint = collision.contacts[0].point;
        Managers.InfoUI.ShowDamageEffect(hitPoint, damage);
        return true;
    }

    void HitEffect(Collision collision)
    {
        if (_hitEffect.activeSelf)
            _hitEffect.SetActive(false);
        _hitEffect.transform.position = collision.contacts[0].point;
        _hitEffect.SetActive(true);
        if (Vector3.Dot(collision.transform.forward, collision.transform.position) > 0)
        {
            collision.rigidbody.AddForce(transform.forward * -3000);
            _hitEffect.transform.rotation = Quaternion.Euler(-90, 0, 0);
        }
        else
        {
            collision.rigidbody.AddForce(transform.forward * 3000);
            _hitEffect.transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
