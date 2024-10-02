using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarFlame : MonoBehaviour, IDetectionObject
{
    [SerializeField]
    Transform _center;
    [SerializeField]
    List<GameObject> _activationObjects;
    [SerializeField]
    LineRenderer _laser;

    int _detectionObjectMask;
    int _connectedCount;
    public Transform RingCenter => _center;

    const float LASER_MAX_DISTANCE = 15;
    const int MAX_INCOMING_CONNECTIONS = 1;
    const int MAX_OUTGOING_CONNECTIONS = 2;

    IDetectionObject[] _detectionObjects;
    RaycastHit[] _detectionRayHits = new RaycastHit[MAX_OUTGOING_CONNECTIONS];

    void Start()
    {
        _detectionObjectMask = LayerMask.GetMask("DetectionObject");
        foreach (var activationObject in _activationObjects)
            activationObject.gameObject.SetActive(false);
        _laser.gameObject.SetActive(false);
        _detectionObjects = new IDetectionObject[MAX_OUTGOING_CONNECTIONS];
    }

    public void Exit()
    {
        if (_connectedCount == 0)
            return;
        _connectedCount = 0;
        foreach (var activationObject in _activationObjects)
            activationObject.SetActive(false);

        _laser.gameObject.SetActive(false);
        for (int i = 0; i < MAX_OUTGOING_CONNECTIONS; i++)
        {
            if (_detectionObjects[i] == null)
                return;
            _detectionObjects[i].Exit();
            _detectionObjects[i] = null;
        }
    }

    public void Reaction()
    {
        if (_connectedCount >= MAX_INCOMING_CONNECTIONS)
            return;
        _connectedCount++;
        _laser.gameObject.SetActive(true);
        int length = Physics.RaycastNonAlloc(_laser.transform.position, _laser.transform.forward, _detectionRayHits, LASER_MAX_DISTANCE, _detectionObjectMask);
        if (length > 0)
        {
            float maxDistance = 0;
            for (int i = 0; i < length; i++)
            {
                _detectionObjects[i] = Util.FindParent<IDetectionObject>(_detectionRayHits[i].transform.gameObject);
                _detectionObjects[i].Reaction();
                if (maxDistance < _detectionRayHits[i].distance)
                    maxDistance = _detectionRayHits[i].distance;
            }
            _laser.SetPosition(1, Vector3.forward * (maxDistance + 0.5f));
        }
        else
        {
            _laser.SetPosition(1, Vector3.forward * LASER_MAX_DISTANCE);
        }

        foreach (var activationObject in _activationObjects)
            activationObject.SetActive(true);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(_laser.transform.position, _laser.transform.forward);
    }
}
