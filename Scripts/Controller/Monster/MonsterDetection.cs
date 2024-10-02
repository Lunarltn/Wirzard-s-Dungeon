using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDetection : BaseDetection
{
    const float ENEMY_DETECTION_RANGE = 10;

    protected override bool Init()
    {
        enemyMask = Managers.Layer.PlayerLayerMask | Managers.Layer.NPCLayerMask;
        ChangeDetectionRangeOption(ENEMY_DETECTION_RANGE, 90);
        ChangeAttackRange(0.7f, 1.0f);
        return true;
    }
}
