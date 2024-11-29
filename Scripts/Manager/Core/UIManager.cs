using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager
{
    int _popupOrder = 10;

    Dictionary<string, UI_Popup> _popupDic = new Dictionary<string, UI_Popup>();

    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
            {
                root = new GameObject { name = "@UI_Root" };
            }
            return root;
        }
    }

    public void SetCanvas(GameObject go, bool sort = true)
    {
        Canvas canvas = go.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        CanvasScaler canvasScaler = go.GetOrAddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);

        if (sort)
        {
            canvas.sortingOrder = _popupOrder;
            _popupOrder++;
        }
    }

    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = Managers.Resource.Instantiate($"UI/Scene/{name}");
        T scene = go.GetOrAddComponent<T>();

        go.transform.SetParent(Root.transform);

        return scene;
    }

    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        if (_popupDic.ContainsKey(name))
        {
            if (_popupDic[name].gameObject.activeSelf == false)
                _popupDic[name].gameObject.SetActive(true);
            return (T)_popupDic[name];
        }

        GameObject go = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = go.GetOrAddComponent<T>();
        _popupDic.Add(name, popup);

        go.transform.SetParent(Root.transform);

        return popup;
    }

    public void ClosePopupUI<T>(string name = null)
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        if (_popupDic.ContainsKey(name) == false)
            return;
        UI_Popup popup = _popupDic[name];
        popup.gameObject.SetActive(false);

        _popupOrder--;
    }

    public void CloseAllPopupUI()
    {
        foreach (string key in _popupDic.Keys)
        {
            if (_popupDic[key].gameObject.activeSelf)
            {
                _popupDic[key].gameObject.SetActive(false);
            }
        }
        _popupOrder = 10;
    }

    public void SortPopup(GameObject go)
    {
        Canvas canvas = go.GetComponent<Canvas>();
        if (_popupOrder > 20)
        {
            _popupOrder = 0;
            foreach (string key in _popupDic.Keys)
            {
                if (_popupDic[key].gameObject.activeSelf)
                {
                    _popupDic[key].gameObject.GetComponent<Canvas>().sortingOrder -= 10;
                    if (_popupOrder < _popupDic[key].gameObject.GetComponent<Canvas>().sortingOrder)
                        _popupOrder = _popupDic[key].gameObject.GetComponent<Canvas>().sortingOrder;
                }
            }
            _popupOrder++;
        }
        canvas.sortingOrder = _popupOrder;
        _popupOrder++;
    }

    public void Clear()
    {
        CloseAllPopupUI();
        _popupDic.Clear();
    }
}