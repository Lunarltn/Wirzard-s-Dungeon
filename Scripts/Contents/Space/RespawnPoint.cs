using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    const float INIT_RADIUS = 3;
    public bool InitRespawn = false;

    const string ALARM = "리스폰이 기록됐습니다.";
    private void Start()
    {
        if (InitRespawn)
            Managers.PlayerInfo.SetRespawnPoint(transform);
        GetComponent<SphereCollider>().radius = INIT_RADIUS;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (Managers.PlayerInfo.GetRespawnPoint() == transform)
            return;
        Managers.InfoUI.ShowAlarm(ALARM);
        Managers.PlayerInfo.SetRespawnPoint(transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, INIT_RADIUS);
    }
}
