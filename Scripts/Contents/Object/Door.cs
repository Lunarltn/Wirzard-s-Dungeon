using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour, IInteraction
{
    Animator _animator;
    protected readonly int OPEN_HASH = Animator.StringToHash("Open");
    protected readonly int FOWARD_HASH = Animator.StringToHash("Foward");
    [SerializeField]
    bool _isLock;
    [SerializeField]
    int _keyNum = 3005;
    bool _usedKey;
    bool _isOpen;
    public bool IsOpen => _isOpen;
    public bool IsLock => _isLock;
    public bool UsedKey => _usedKey;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void Lock() => _isLock = true;
    public void UnLock() => _isLock = false;
    public void CloseDoor()
    {
        _isOpen = false;
        _animator.SetBool(OPEN_HASH, false);
    }
    public void OpenDoor()
    {
        _isOpen = true;
        _animator.SetBool(OPEN_HASH, true);
    }

    public void Interect()
    {
        if (_isLock)
        {
            int keyIndex = Managers.Inventory.SearchItemIndex(Define.MainCategory.Etc, _keyNum);
            if (keyIndex != -1)
            {
                Managers.Inventory.DropItem(Define.MainCategory.Etc, keyIndex);
                UnLock();
                _usedKey = true;
                Managers.InfoUI.ShowAlarm("���� ���Ƚ��ϴ�.");
            }
        }
        else
        {
            if (_isOpen)
                CloseDoor();
            else
                OpenDoor();
        }
    }
    public string GetInterectionName()
    {
        return "��";
    }

    public string GetInterectionMessage()
    {
        if (_isLock)
        {
            int keyIndex = Managers.Inventory.SearchItemIndex(Define.MainCategory.Etc, _keyNum);
            if (keyIndex == -1)
                return "���";
            else
                return "����";
        }
        else
        {
            if (_isOpen)
                return "�ݱ�";
            else
                return "����";
        }
    }

    public void SetPosition(Vector3 position)
    {
        _animator.SetBool(FOWARD_HASH,
            Vector3.Dot((position - transform.position).normalized, transform.forward) < 0);
    }
}
