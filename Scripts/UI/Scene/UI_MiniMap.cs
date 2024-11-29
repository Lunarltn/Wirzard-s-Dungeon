using RootMotion.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_MiniMap : UI_Scene
{
    enum Images
    {
        Image_QuestCurser
    }
    bool _isActiveCurser;
    Transform _monsters;
    Transform _items;
    Transform _questCurserTarget;
    Quest _questData;

    public override bool Init()
    {
        BindImage(typeof(Images));
        _monsters = GameObject.Find("@Monster").transform;
        _items = GameObject.Find("@Item").transform;
        HideQuestCurser();
        return true;
    }

    private void Update()
    {
        if (_isActiveCurser)
        {
            UpdateQuestCurserTarget();

            if (_questCurserTarget != null && _questCurserTarget.gameObject.activeSelf && Managers.PlayerInfo.Player != null)
            {
                GetImage((int)Images.Image_QuestCurser).gameObject.SetActive(true);
                var dir = (_questCurserTarget.position - Managers.PlayerInfo.Player.transform.position).normalized;
                var z = Util.Vector3ToAngle(dir);
                RotateQuestCurser(z - 90);
            }
            else
            {
                GetImage((int)Images.Image_QuestCurser).gameObject.SetActive(false);
            }
        }
    }

    public void BindQuest(Quest quest)
    {
        _questData = quest;
        UpdateQuestCurserTarget();
    }

    void UpdateQuestCurserTarget()
    {
        if (_questData == null) return;
        switch (_questData.Category)
        {
            case Define.QuestCategory.Hunt:
                float min = float.MaxValue;
                for (int i = 0; i < _monsters.childCount; i++)
                {
                    float dist = Vector3.SqrMagnitude(_monsters.GetChild(i).transform.position - Managers.PlayerInfo.Player.transform.position);
                    if (min > dist)
                    {
                        min = dist;
                        var controller = _monsters.GetChild(i).GetComponent<BaseController>();
                        if (controller != null && controller.IsDead == false)
                        {
                            for (int j = 0; j < _questData.RequestCount; j++)
                            {
                                if (_questData.IsCompleteRequest(j) == false && controller.ID == _questData.GetRequestID(j))
                                    _questCurserTarget = controller.transform;
                                else
                                    _questCurserTarget = null;

                                if (_questCurserTarget != null)
                                    break;
                            }
                        }
                    }
                }
                break;
            case Define.QuestCategory.Collect:
                for (int i = 0; i < _items.childCount; i++)
                {
                    var item = _items.GetChild(i).GetComponent<DropItem>();
                    if (item != null)
                    {
                        for (int j = 0; j < _questData.RequestCount; j++)
                        {
                            if (item.ID == _questData.GetRequestID(j))
                                _questCurserTarget = item.transform;
                        }
                    }
                }
                break;
        }
    }

    public void ShowQuestCurser()
    {
        _isActiveCurser = true;
        GetImage((int)Images.Image_QuestCurser).gameObject.SetActive(true);
    }

    public void HideQuestCurser()
    {
        _isActiveCurser = false;
        GetImage((int)Images.Image_QuestCurser).gameObject.SetActive(false);
    }

    public void RotateQuestCurser(float z)
    {
        GetImage((int)Images.Image_QuestCurser).rectTransform.rotation = Quaternion.Euler(0, 0, z);
    }
}
