using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyController : MonsterController
{
    const float RESPAWN_TIME = 5f;
    float _respawnTimer;

    protected override void Init()
    {
        ID = 0;
        base.Init();
    }

    private void Update()
    {
        if (_respawnTimer > 0)
            _respawnTimer -= Time.deltaTime;
        if (_respawnTimer <= 0 && IsDead)
        {
            IsDead = false;
            animator.SetBool(DIE_HASH, false);
        }

    }

    protected override void Die()
    {
        animator.SetBool(DIE_HASH, true);
        _respawnTimer = RESPAWN_TIME;
        IsDead = true;
    }
}
