using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    const float DESTROY_TIME = 10;
    List<Rigidbody> _debris = new List<Rigidbody>();
    bool _isDestroy;
    float _startTime;
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("DestructibleObject");

        transform.GetChild(0).gameObject.SetActive(false);

        foreach (Transform child in transform.GetChild(0))
        {
            _debris.Add(child.GetComponent<Rigidbody>());
        }
    }

    private void OnEnable()
    {
        _startTime = Time.time;
    }

    private void Update()
    {
        if (Time.time - _startTime > DESTROY_TIME)
            Managers.Resource.Destroy(gameObject);
    }

    public virtual void Destroy(float force, Vector3 explosionPosition)
    {
        if (_isDestroy) return; _isDestroy = true;
        GetComponent<MeshCollider>().enabled = false;
        GetComponent<MeshRenderer>().enabled = false;
        transform.GetChild(0).gameObject.SetActive(true);
        AddExplosionForceAtDebris(force, explosionPosition);
    }

    void AddExplosionForceAtDebris(float force, Vector3 explosionPosition)
    {
        foreach (Rigidbody rigidbody in _debris)
        {
            rigidbody.AddExplosionForce(force, explosionPosition, 5, 0, ForceMode.Impulse);
        }
    }

}
