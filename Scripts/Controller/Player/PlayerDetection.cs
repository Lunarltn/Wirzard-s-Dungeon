using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerDetection : MonoBehaviour
{
    const int NON_ALLOC_SIZE = 5;
    const float DETECTION_HEIGHT = 0.9f;
    const float DETECTION_MAX_DISTANCE = 2;
    const float DETECTIION_HEAD_HEIGHT = 0.7f;
    readonly Vector3 _detectedHeadBoxSize = new Vector3(0.35f, 0.1f, 0.35f);
    readonly Vector3 _detectedFrontSize = new Vector3(1.4f, 1.6f, 0.8f);
    readonly Vector3 _detectedKnockbackBoxSize = new Vector3(1.4f, 0.5f, 2f);
    Vector3 _detectedFrontPosition => transform.position - transform.forward * 0.2f + Vector3.up;
    Vector3 _detectedKnockbackPosition => transform.position + Vector3.forward * 0.8f;

    Collider[] _kickedEnemyColliders = new Collider[NON_ALLOC_SIZE];
    RaycastHit[] _interactionHits = new RaycastHit[NON_ALLOC_SIZE];

    RaycastHit _groundRayHit;

    [SerializeField]
    int _maxSlopeAngle = 50;
    int _detectedInteractionLayerCount;
    int _detectedKickLayerCount;

    public Collider[] KickedEnemyColliders => _kickedEnemyColliders;

    public bool IsOnGround
    {
        get
        {
            return Physics.BoxCast(transform.position + Vector3.up, _detectedHeadBoxSize, Vector3.down, out _groundRayHit, transform.rotation, DETECTION_HEIGHT, ~(Managers.Layer.PlayerLayerMask | Managers.Layer.MonsterLayerMask));
        }
    }

    public bool IsOnSlope
    {
        get
        {
            var angle = Vector3.Angle(Vector3.up, _groundRayHit.normal);
            return angle != 0 && angle < _maxSlopeAngle;
        }
    }
    public bool IsOnMarginalSlope
    {
        get
        {
            var angle = Vector3.Angle(Vector3.up, _groundRayHit.normal);
            return angle >= _maxSlopeAngle;
        }
    }

    public Vector3 DirectionToSlope(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, _groundRayHit.normal).normalized;
    }

    public bool IsAboveHead => Physics.BoxCast(transform.position + Vector3.up, _detectedHeadBoxSize, transform.up, transform.rotation, DETECTIION_HEAD_HEIGHT);

    public bool DetectInteractionRange()
    {
        _detectedInteractionLayerCount = Physics.BoxCastNonAlloc(_detectedFrontPosition, _detectedFrontSize / 2
            , transform.forward, _interactionHits, transform.rotation, DETECTION_MAX_DISTANCE, Managers.Layer.InterectionLayerMask | Managers.Layer.NPCLayerMask, QueryTriggerInteraction.Collide);
        return _detectedInteractionLayerCount > 0;
    }

    public IInteraction GetINearbyInteraction()
    {
        float min = float.MaxValue;
        RaycastHit nearbyHit = new RaycastHit();
        for (int i = 0; i < _detectedInteractionLayerCount; i++)
        {
            if (_interactionHits[i].transform != null)
            {
                if (_interactionHits[i].distance < min)
                {
                    min = _interactionHits[i].distance;
                    nearbyHit = _interactionHits[i];
                }
            }
            else break;
        }

        if (nearbyHit.transform == null)
        {
            return null;
        }
        return nearbyHit.transform.GetComponent<IInteraction>();
    }

    public int DetectKickRange()
    {
        var pos = _detectedFrontPosition + transform.forward * DETECTION_MAX_DISTANCE;
        _detectedKickLayerCount = Physics.OverlapBoxNonAlloc(pos, _detectedFrontSize / 2
            , _kickedEnemyColliders, transform.rotation, Managers.Layer.MonsterLayerMask);
        return _detectedKickLayerCount;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(Vector3.up - transform.forward * 0.2f, Vector3.forward * DETECTION_MAX_DISTANCE);
        Gizmos.DrawWireCube(Vector3.forward * DETECTION_MAX_DISTANCE + Vector3.up - transform.forward * 0.2f - Vector3.forward * _detectedFrontSize.z * 0.5f,
            _detectedFrontSize);
        //Gizmos.DrawWireCube(_detectedKnockbackPosition - transform.position, _detectedKnockbackBoxSize);

    }
}
