using UnityEngine;
using UnityEngine.UI;

public class Slot_EquipmentItem : MonoBehaviour
{
    public Define.EquipCategory Category;
    public bool IsNull;
    public Image Image;
    readonly Color32 baseColor = new Color32(103, 103, 103, 187);

    public void Init(Define.EquipCategory category)
    {
        Category = category;
        IsNull = true;
        Image = GetComponent<Image>();
        SetImageColor();
    }

    public void SetImageColor()
    {
        if (IsNull)
            Image.color = baseColor;
        else
            Image.color = Color.white;
    }
}
