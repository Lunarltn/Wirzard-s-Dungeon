using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class MovingScaffolding : MonoBehaviour
{
    [SerializeField]
    Vector3[] _destination = new Vector3[2];
    Vector3 _dir;
    bool _isReverse;
    const float WAITING_TIME = 3;
    float _waitingTimer;

    void FixedUpdate()
    {
        if (_isReverse)
        {
            if (Vector3.Distance(transform.position, _destination[0]) < 0.1f)
            {
                _dir = Vector3.zero;
                if (_waitingTimer > WAITING_TIME)
                {
                    _waitingTimer = 0;
                    _isReverse = false;
                }
                else
                    _waitingTimer += Time.deltaTime;
            }
            else
                _dir = Vector3.forward * Time.deltaTime * 5;
        }
        else
        {
            if (Vector3.Distance(transform.position, _destination[1]) < 0.1f)
            {
                _dir = Vector3.zero;
                if (_waitingTimer > WAITING_TIME)
                {
                    _waitingTimer = 0;
                    _isReverse = true;
                }
                else
                    _waitingTimer += Time.deltaTime;
            }
            else
                _dir = Vector3.back * Time.deltaTime * 5;
        }
        transform.position += _dir;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer == Managers.Layer.PlayerLayer)
        {
            collision.transform.position += _dir;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_destination[0], 0.5f);
        Gizmos.DrawSphere(_destination[1], 0.5f);
        Gizmos.DrawLine(_destination[0], _destination[1]);

    }
}
