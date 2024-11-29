using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest
{
    const int MAX_ARRAY = 6;

    public int RequestCount => _requests.Length == 0 ? 0 : _requests.Length / 2;
    public int RewardCount => _rewards.Length == 0 ? 0 : _rewards.Length / 2;
    public int ID => _id;
    public int NPC => Managers.Data.QuestDic[_id].npc;
    public Define.QuestCategory Category => (Define.QuestCategory)Managers.Data.QuestDic[_id].category;
    public Item[] RequestItems { get; private set; }

    Define.QuestStatus _status;
    Action<int, Define.QuestStatus> _changeStatus;
    Action<Quest> _updateStatus;
    int _id;
    int[] _requests;
    int[] _rewards;
    int[] _currentRequests;

    public Define.QuestStatus Status
    {
        get { return _status; }
        set { _status = value; }
    }

    public Quest(int id, Define.QuestStatus status, Action<int, Define.QuestStatus> changeStatus, Action<Quest> updateStatus)
    {
        _id = id;
        _status = status;
        _requests = Util.SplitItemString(Managers.Data.QuestDic[id].request);
        _rewards = Util.SplitItemString(Managers.Data.QuestDic[id].rewards);
        _currentRequests = new int[MAX_ARRAY];
        _changeStatus = changeStatus;
        _updateStatus = updateStatus;
        RequestItems = new Item[RequestCount];
        for (int i = 0; i < RequestCount; i++)
            RequestItems[i] = new Item(_requests[i * 2], _requests[i * 2 + 1]);
    }

    void CompletedQuest()
    {
        _changeStatus?.Invoke(ID, Define.QuestStatus.Can_Finish);
    }

    public string GetRequestName(int index)
    {
        if (RequestCount == 0) return null;

        int idx = index * 2;
        switch (Category)
        {
            case Define.QuestCategory.Hunt:
                return Managers.Data.MonsterDic[_requests[idx]].name;
            case Define.QuestCategory.Collect:
                switch (Mathf.FloorToInt(_requests[idx] / 1000))
                {
                    case 1:
                        return Managers.Data.EquipItemDic[_requests[idx]].name;
                    case 2:
                        return Managers.Data.UseItemDic[_requests[idx]].name;
                    case 3:
                        return Managers.Data.EtcItemDic[_requests[idx]].name;
                }
                break;
        }
        return null;
    }

    public bool CompleateCollectionRequest()
    {
        if (Category != Define.QuestCategory.Collect)
            return true;

        for (int i = 0; i < RequestCount; i++)
        {
            int index = Managers.Inventory.SearchItemIndex(RequestItems[i].MainCategory, RequestItems[i].Num);
            if (index != -1)
            {
                Managers.Inventory.DropItem(RequestItems[i].MainCategory, index, RequestItems[i].Count);
            }
        }
        return true;
    }

    public void UpdateRequest(int rquestID, int count)
    {
        if (RequestCount == 0) return;

        if (!(_status == Define.QuestStatus.In_Progress || _status == Define.QuestStatus.Can_Finish))
            return;

        for (int i = 0; i < RequestCount; i++)
        {
            if (_requests[i * 2] == rquestID)
                AccumulateProgressRequest(i, count);
        }
    }

    void AccumulateProgressRequest(int index, int count)
    {
        if (RequestCount == 0) return;
        _currentRequests[index] += count;
        _updateStatus?.Invoke(this);
        int completeCount = 0;
        for (int i = 0; i < RequestCount; i++)
        {
            if (_currentRequests[i] >= _requests[i * 2 + 1])
                completeCount++;
        }

        if (completeCount == RequestCount)
            CompletedQuest();
    }
    //QuestStatusUI
    public int GetRequestID(int index) => RequestCount == 0 ? 0 : _requests[index * 2];
    public int GetRequestCount(int index) => RequestCount == 0 ? 0 : _requests[index * 2 + 1];
    public int GetProgressRequestCount(int index) => RequestCount == 0 ? 0 : _currentRequests[index];
    public bool IsCompleteRequest(int index) => GetRequestCount(index) <= GetProgressRequestCount(index);
    public void GetReward()
    {
        for (int i = 0; i < RewardCount; i++)
        {
            if (_rewards[i * 2] == 0)
                Managers.Inventory.AddGold(_rewards[i * 2 + 1]);
            else
                Managers.Inventory.AddItem(new Item(_rewards[i * 2], _rewards[i * 2 + 1]));
        }
    }
}

public class QuestManager
{
    public bool IsOpenQuestWindow => _isOpenQuestWindow;

    UI_QuestWindow _uI_QuestWindow;
    UI_QuestMark _uI_QuestMark;
    UI_QuestStatus _uI_QuestStatus;
    UI_MiniMap _uI_MiniMap;
    Dictionary<int, Quest> _quests;
    Dictionary<int, int> _preQuests;
    Action _closeAction;
    List<int> _proceedingQuestID;
    bool _isOpenQuestWindow;
    bool _isOpenQuestStatus;

    public void Init()
    {
        _uI_QuestWindow = Managers.UI.ShowSceneUI<UI_QuestWindow>();
        _uI_QuestMark = Managers.UI.ShowSceneUI<UI_QuestMark>();
        _uI_QuestStatus = Managers.UI.ShowSceneUI<UI_QuestStatus>();
        _uI_MiniMap = Managers.UI.ShowSceneUI<UI_MiniMap>();
        _quests = new Dictionary<int, Quest>();
        _preQuests = new Dictionary<int, int>();
        _proceedingQuestID = new List<int>();

        foreach (int key in Managers.Data.QuestDic.Keys)
        {
            _quests.Add(key, new Quest(key, Define.QuestStatus.None, ChangeQuestStatus, UpdateQuestRequest));

            if (Managers.Data.QuestDic[key].preQuest != 0)
                _preQuests.Add(Managers.Data.QuestDic[key].preQuest, key);
            else
                ChangeQuestStatus(key, Define.QuestStatus.Can_Start);
        }
    }

    Define.QuestStatus GetQuestStatusAtQuestID(int questId)
    {
        if (_quests.ContainsKey(questId)) return _quests[questId].Status;
        else return Define.QuestStatus.None;
    }

    public int CanStartQuestIDAtNPCID(int npcId)
    {
        foreach (int key in _quests.Keys)
        {
            if (_quests[key].NPC == npcId &&
                _quests[key].Status == Define.QuestStatus.Can_Start)
                return key;
        }
        return 0;
    }

    public int CanFinishQuestIDAtNPCID(int npcId)
    {
        foreach (int key in _quests.Keys)
        {
            if (_quests[key].NPC == npcId &&
                _quests[key].Status == Define.QuestStatus.Can_Finish)
                return key;
        }
        return 0;
    }

    void ChangeQuestStatus(int questID, Define.QuestStatus questStatus)
    {
        if (_quests.ContainsKey(questID) == false) return;

        _quests[questID].Status = questStatus;
        ChangeQuestMark(_quests[questID].NPC, questStatus);

        switch (questStatus)
        {
            case Define.QuestStatus.Can_Start:
                break;
            case Define.QuestStatus.In_Progress:
                ShowQuestCurser();
                UpdateQuestCurser(_quests[questID]);
                _proceedingQuestID.Add(questID);
                if (_isOpenQuestStatus)
                    UpdateQuestStatus(_quests[questID]);
                else
                    ShowQuestStatus(_quests[questID]);
                break;
            case Define.QuestStatus.Finished:
                _proceedingQuestID.Remove(questID);
                HideQuestStatus();
                if (_preQuests.ContainsKey(questID))
                    ChangeQuestStatus(_preQuests[questID], Define.QuestStatus.Can_Start);
                else
                    Managers.InfoUI.ShowGameClearWindow();
                break;
        }
    }

    void CheckInventory(int questID)
    {
        if (_quests.ContainsKey(questID) == false
            || _quests[questID].Category != Define.QuestCategory.Collect)
            return;

        var quest = _quests[questID];
        for (int i = 0; i < quest.RequestCount; i++)
        {
            var item = quest.RequestItems[i];
            int inventoryIndex = Managers.Inventory.SearchItemIndex(item.MainCategory, item.Num);
            if (inventoryIndex != -1)
            {
                var inventoryItem = Managers.Inventory.GetItem(item.MainCategory, inventoryIndex);
                CountQuestRequest(Define.QuestCategory.Collect, inventoryItem.Num, inventoryItem.Count);
            }
        }
    }

    public void CountQuestRequest(Define.QuestCategory questCategory, int requestID, int count)
    {
        for (int i = 0; i < _proceedingQuestID.Count; i++)
        {
            int questID = _proceedingQuestID[i];
            if (_quests[questID].Category == questCategory)
            {
                _quests[questID].UpdateRequest(requestID, count);
            }
        }
    }

    public void CompleteQuest(int questID)
    {
        if (_quests[questID].CompleateCollectionRequest() == false)
            return;
        ChangeQuestStatus(questID, Define.QuestStatus.Finished);
        _quests[questID].GetReward();
    }

    public void OpenQuestWindow(Transform npc, Transform cameraTarget, int npcID, Action closeAction)
    {
        int questID = CanStartQuestIDAtNPCID(npcID);
        _isOpenQuestWindow = true;
        _uI_QuestWindow.ShowQuestWindow(questID);
        Managers.Input.UnlockMouse();
        Managers.Camera.OnEnableCinemachineViewNPC(npc, cameraTarget);
        _closeAction = closeAction;
    }

    public void CloseQuestWindow()
    {
        _isOpenQuestWindow = false;
        _closeAction?.Invoke();
        Managers.Input.LockMouse();
        Managers.Camera.OnDisableCinemachineViewNPC();
    }

    public void AcceptQuest(int questID)
    {
        if (questID == 0) return;
        ChangeQuestStatus(questID, Define.QuestStatus.In_Progress);
        CheckInventory(questID);
        CloseQuestWindow();
    }

    void ChangeQuestMark(int npcIndex, Define.QuestStatus questState)
    {
        _uI_QuestMark.ChangeQuestMark(npcIndex, questState);
    }

    void UpdateQuestRequest(Quest quest)
    {
        _uI_QuestStatus.UpdateQuestRequest(quest);
    }

    void UpdateQuestStatus(Quest quest)
    {
        _uI_QuestStatus.UpdateQuestStatus(quest);
    }

    void ShowQuestStatus(Quest quest)
    {
        _isOpenQuestStatus = true;
        _uI_QuestStatus.ShowQuestStatus(quest);
    }

    void HideQuestStatus()
    {
        _isOpenQuestStatus = false;
        _uI_QuestStatus.HideQuestStatus();
    }

    void UpdateQuestCurser(Quest quest)
    {
        _uI_MiniMap.BindQuest(quest);
    }

    void ShowQuestCurser()
    {
        _uI_MiniMap.ShowQuestCurser();
    }

    void HideQuestCurser()
    {
        _uI_MiniMap.HideQuestCurser();
    }
}
