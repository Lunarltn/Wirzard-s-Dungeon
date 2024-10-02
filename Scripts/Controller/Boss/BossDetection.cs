using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class BossDetection : BaseDetection
{
    public BossRoom BossRoom;

    protected override bool Init()
    {
        enemyMask = (1 << Managers.Layer.PlayerLayer) | (1 << Managers.Layer.NPCLayer);
        ChangeDetectionRangeOption(10, 90);
        ChangeAttackRange(1.7f, 5);
        return true;
    }

    public bool DetectPlayerInBossRoom()
    {
        Rigidbody target = null;
        float min = float.MaxValue;
        var count = Physics.OverlapBoxNonAlloc(BossRoom.RoomPosition, (BossRoom.RoomSizeV3 + Vector3.up * 10) * 0.5f, detectedEnemyInDetectionRange, Quaternion.identity, Managers.Layer.PlayerLayerMask);

        for (int i = 0; i < count; i++)
        {
            if (detectedEnemyInDetectionRange[i] != null)
            {
                var dist = Vector3.Distance(transform.position, detectedEnemyInDetectionRange[i].transform.position);
                if (dist < min)
                {
                    min = dist;
                    target = detectedEnemyInDetectionRange[i].attachedRigidbody;
                }
            }
        }

        EnemyInDetectionRange = target;
        return target != null;
    }

    public float GetDistanceBetweenPlayer()
    {
        return Vector3.Distance(transform.position, Managers.PlayerInfo.Player.transform.position);
    }

    public Vector3 GetPlayerDirection()
    {
        return (Managers.PlayerInfo.Player.transform.position - transform.position).normalized;
    }

    public bool FindVisiblePlayer(float viewingAngle, float minRange = 0)
    {
        float targetAngle = Mathf.Acos(Vector3.Dot(transform.forward, GetPlayerDirection())) * Mathf.Rad2Deg;
        return (targetAngle <= viewingAngle * 0.5f && GetDistanceBetweenPlayer() > minRange);
    }
}
