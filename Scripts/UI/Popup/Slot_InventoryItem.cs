using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot_InventoryItem : MonoBehaviour
{
    bool isNull;
    public Define.MainCategory category;
    public int Index;
    public bool IsNull
    {
        get { return isNull; }
        set
        {
            isNull = value;
            var color = Image.color;
            if (isNull)
                color.a = 0;
            else
                color.a = 1;
            Image.color = color;
            Count.color = color;
        }
    }
    public Image Image;
    public TextMeshProUGUI Count;

    public void Init(int idx)
    {
        Index = idx;
        Image = GetComponent<Image>();
        Count = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        IsNull = true;
    }
}

