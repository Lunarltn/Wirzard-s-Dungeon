using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager
{
    Dictionary<int, Action> _actions = new Dictionary<int, Action>();

    public void GetAction(int key, Action action)
    {
        if (_actions.ContainsKey(key))
            return;

        if (key > 0 && key < 10)
        {
            Debug.Log(key + " µî·Ï");
            _actions.Add(key, action);
        }
    }

    public void Update()
    {
        foreach (var key in _actions.Keys)
        {
            if (Input.GetKeyUp((KeyCode)256 + key)) _actions[key].Invoke();
        }
    }
}
