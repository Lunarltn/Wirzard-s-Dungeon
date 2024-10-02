using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    //keyboard input
    public bool IsJump { get; private set; }
    public bool IsRun { get; private set; }
    public bool IsCrouch { get; private set; }
    public bool IsDodge { get; private set; }
    public bool IsInterect { get; private set; }
    public Vector3 MoveDirection { get; private set; }
    bool _isMove;
    bool _isScreenLock;
    bool isScreenLock
    {
        get { return _isScreenLock; }
        set
        {
            _isScreenLock = value;
            if (value == false)
            {
                MouseLeftClick = false;
                MouseRightClick = false;
            }
        }
    }
    int _lockStack;
    Dictionary<KeyCode, bool> _isOpenPopup;
    Dictionary<KeyCode, Action<int>> _hotKeys;

    Texture2D[] _corsor;

    //mouse input
    public bool MouseLeftClick;
    public bool MouseRightClick { get; private set; }
    public float MouseWheel { get; private set; }

    public bool IsMouseLeftClick() => MouseLeftClick;
    public void Init()
    {
        PlayMove();
        LockMouse();
        _isOpenPopup = new Dictionary<KeyCode, bool>();
        _hotKeys = new Dictionary<KeyCode, Action<int>>();
        _corsor = new Texture2D[2];
        _corsor[0] = Managers.Resource.Load<Texture2D>("Cursor/Cursor_Basic");
        _corsor[1] = Managers.Resource.Load<Texture2D>("Cursor/Cursor_Hand");
    }

    void RegisterPopupKey<T>(KeyCode keyCode) where T : UI_Popup
    {
        if (_isOpenPopup.ContainsKey(keyCode) == false)
            _isOpenPopup.Add(keyCode, false);

        if (_isOpenPopup.ContainsKey(KeyCode.Escape) && _isOpenPopup[KeyCode.Escape] && keyCode != KeyCode.Escape)
            return;

        if (Input.GetKeyUp(keyCode) && _isOpenPopup[keyCode] == false)
        {
            Managers.UI.ShowPopupUI<T>();
            OpenPopupKeyCode(keyCode);
        }
        else if (Input.GetKeyUp(keyCode) && _isOpenPopup[keyCode] == true)
        {
            Managers.UI.ClosePopupUI<T>();
            Managers.Inventory.UI_InventoryAssistent.CloseItemInfo();
            Managers.Inventory.UI_InventoryAssistent.DisabledImage();
            ClosePopupKeyCode(keyCode);
        }
    }
    void OpenPopupKeyCode(KeyCode keyCode)
    {
        if (_isOpenPopup.ContainsKey(keyCode) == false)
            return;
        _isOpenPopup[keyCode] = true;
        UnlockMouse();
    }

    public void ClosePopupKeyCode(KeyCode keyCode)
    {
        if (_isOpenPopup.ContainsKey(keyCode) == false)
            return;
        if (keyCode == KeyCode.Escape)
            Time.timeScale = 1;

        _isOpenPopup[keyCode] = false;
        LockMouse();
    }

    void RegisterSkillHotKey(int num)
    {
        if (_hotKeys.ContainsKey((KeyCode)48 + num) == false)
            _hotKeys.Add((KeyCode)48 + num, Managers.HotKey.ChangeCurrentSkill);

        if (Input.GetKey((KeyCode)48 + num))
        {
            _hotKeys[(KeyCode)48 + num](num - 1);
        }
    }

    void RegisterItemHotKey(KeyCode keyCode, int idx)
    {
        if (_hotKeys.ContainsKey(keyCode) == false)
            _hotKeys.Add(keyCode, Managers.HotKey.UseItem);

        if (Input.GetKey(keyCode))
        {
            _hotKeys[keyCode](idx);
        }
    }

    public void HandleInput()
    {
        if (Managers.PlayerInfo != null && Managers.PlayerInfo.IsDead)
            return;
        RegisterPopupKey<UI_Inventory>(KeyCode.I);
        RegisterPopupKey<UI_Equipment>(KeyCode.E);
        RegisterPopupKey<UI_Skill>(KeyCode.K);
        RegisterPopupKey<UI_Pause>(KeyCode.Escape);

        RegisterItemHotKey(KeyCode.Z, 0);
        RegisterItemHotKey(KeyCode.X, 1);
        RegisterSkillHotKey(1);
        RegisterSkillHotKey(2);
        RegisterSkillHotKey(3);
        RegisterSkillHotKey(4);

        MoveDirection = _isMove ? new Vector3
        {
            x = Input.GetAxis("Horizontal"),
            y = 0.0f,
            z = Input.GetAxis("Vertical")
        }
        : Vector3.zero;
        IsJump = Input.GetKey(KeyCode.Space);
        IsRun = Input.GetKey(KeyCode.LeftShift);
        IsCrouch = Input.GetKeyUp(KeyCode.LeftControl);
        IsDodge = Input.GetKey(KeyCode.C);
        IsInterect = Input.GetKeyUp(KeyCode.F);

        if (isScreenLock)
        {
            MouseLeftClick = Input.GetMouseButton(0);
            MouseRightClick = Input.GetMouseButton(1);
        }

        MouseWheel = Input.GetAxis("Mouse ScrollWheel");
    }

    public void LockMouse()
    {
        if (_lockStack > 0)
        {
            _lockStack--;
            return;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Managers.Camera.CameraUnLock();
        isScreenLock = true;
    }

    public void UnlockMouse()
    {
        if (isScreenLock == false)
        {
            _lockStack++;
            return;
        }
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Managers.Camera.CameraLock();
        isScreenLock = false;
    }

    public void ChangeMouseCurser(Define.Cursor cursor)
    {
        if (_corsor.Length > (int)cursor)
            Cursor.SetCursor(_corsor[(int)cursor], new Vector2(14, 0), CursorMode.Auto);
    }

    public void StopMove()
    {
        _isMove = false;
    }

    public void PlayMove()
    {
        _isMove = true;
    }
}
