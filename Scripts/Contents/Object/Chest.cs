using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Chest : MonoBehaviour, IInteraction
{
    [Serializable]
    class _item
    {
        [SerializeField]
        int _num;
        [SerializeField]
        int _count;
        public int Num => _num;
        public int Count => _count;
    }
    [SerializeField]
    int _gold;
    [SerializeField]
    List<_item> _items;

    Animator _animator;
    protected readonly int OPEN_HASH = Animator.StringToHash("Open");
    GameObject _coin;
    bool _isOpen;

    private void Start()
    {
        _animator = transform.GetComponent<Animator>();
        _coin = Util.FindChild(gameObject, "GoldCoins");
    }

    public void Interect()
    {
        if (_isOpen)
        {
            if (_coin.activeSelf)
            {
                _coin.SetActive(false);
                Managers.Inventory.AddGold(_gold);
                for (int i = 0; i < _items.Count; i++)
                    Managers.Inventory.AddItem(new Item(_items[i].Num, _items[i].Count));
            }
        }
        else
        {
            _isOpen = true;
            _animator.SetBool(OPEN_HASH, true);
        }
    }

    public string GetInterectionName()
    {
        if (_isOpen)
        {
            if (_coin.activeSelf)
                return "아이템";
            else
                return string.Empty;
        }
        else
            return "상자";

    }

    public string GetInterectionMessage()
    {
        if (_isOpen)
            return "줍기";
        else
            return "열기";
    }

    public void SetPosition(Vector3 position)
    {

    }
}
