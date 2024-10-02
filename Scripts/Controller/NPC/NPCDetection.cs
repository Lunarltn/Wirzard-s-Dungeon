using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDetection : BaseDetection
{
    const float PLAYER_DETECTION_RANGE = 6;
    const float ENEMY_DETECTION_RANGE = 10;

    protected override bool Init()
    {
        enemyMask = Managers.Layer.MonsterLayerMask;
        ChangeDetectionRangeOption(ENEMY_DETECTION_RANGE, 270);
        ChangeAttackRange(0.7f, 1.3f);

        return true;
    }

    public bool DetectPlayer()
    {
        return Vector3.Distance(transform.position, Managers.PlayerInfo.Player.transform.position) < PLAYER_DETECTION_RANGE;
    }
}
