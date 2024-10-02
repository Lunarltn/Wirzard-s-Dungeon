using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fountain : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (Managers.PlayerInfo.Controller.Stat.HP != Managers.PlayerInfo.Controller.Stat.CurrentHP
                || Managers.PlayerInfo.Controller.Stat.MP != Managers.PlayerInfo.Controller.Stat.CurrentMP)
            {
                Managers.PlayerInfo.Controller.Stat.IncreaseHP(1000);
                Managers.PlayerInfo.Controller.Stat.IncreaseMP(1000);

                Managers.InfoUI.ShowAlarm("È¸º¹µÊ");
            }
        }

    }
}
