using UnityEngine;

public class Slot_HotKey : MonoBehaviour
{
    public enum SlotType
    {
        Skill,
        UseItem
    }
    public int Index;
    public SlotType Type;

    public void Init(SlotType tpye, int idx)
    {
        Type = tpye;
        Index = idx;
    }
}
