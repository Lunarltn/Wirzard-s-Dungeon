using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour, IInteraction
{
    public int ID;
    public int Count;

    const string INTERECTION_MESSAGE = "ащ╠Б";
    string _itemName;
    Item _item;

    private void Start()
    {
        _item = new Item(ID, Count);
        _itemName = _item.GetName() + "(" + Count + ")";
    }

    public string GetInterectionMessage()
    {
        return INTERECTION_MESSAGE;
    }

    public string GetInterectionName()
    {
        return _itemName;
    }

    public void Interect()
    {
        Managers.Inventory.AddItem(_item);
        Managers.Resource.Destroy(gameObject);
    }

    public void SetPosition(Vector3 position)
    {
    }
}
