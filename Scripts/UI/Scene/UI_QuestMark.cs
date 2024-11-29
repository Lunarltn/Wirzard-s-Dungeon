using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UI_QuestMark : UI_Scene
{
    enum Images
    {
        Image_Clone
    }

    Dictionary<int, Image> _images = new Dictionary<int, Image>();
    Dictionary<int, bool> _activeMarks = new Dictionary<int, bool>();
    Dictionary<Define.QuestStatus, Sprite> _questSprites = new Dictionary<Define.QuestStatus, Sprite>();
    Dictionary<int, CancellationTokenSource> _cancleTokenSources = new Dictionary<int, CancellationTokenSource>();

    float _activeDistance = 75;

    public override bool Init()
    {
        BindImage(typeof(Images));

        GetImage(0).transform.gameObject.SetActive(false);

        foreach (int key in Managers.Data.NPCDic.Keys)
        {
            _images.Add(key, Instantiate(GetImage(0).transform.gameObject, transform).GetComponent<Image>());
            _activeMarks.Add(key, false);
            _cancleTokenSources.Add(key, new CancellationTokenSource());
        }

        for (int i = 1; i <= 3; i++)
        {
            _questSprites.Add((Define.QuestStatus)i, Managers.Resource.Load<Sprite>($"Sprite/UI/QuestMark{i}"));
        }

        return true;
    }

    void Update()
    {
        foreach (int key in _images.Keys)
        {
            if (_images.ContainsKey(key) == false || Managers.NPC.GetNPC(key) == null)
                return;
            float distance = Vector3.Distance(Managers.PlayerInfo.Player?.transform.position ?? Vector3.zero, Managers.NPC.GetNPC(key).position);
            if (distance < _activeDistance)
            {
                if (_activeMarks[key] && _images[key].gameObject.activeSelf == false)
                    _images[key].gameObject.SetActive(true);
                else if (_activeMarks[key] == false && _images[key].gameObject.activeSelf)
                    _images[key].gameObject.SetActive(false);

                if (_activeMarks[key])
                {
                    UpdateQuestMarkPosition(key);
                    UpdateQuestMarkRotate(key);
                }
            }
            else if (_images[key].gameObject.activeSelf)
                _images[key].gameObject.SetActive(false);
        }
    }

    public void ChangeQuestMark(int npcIndex, Define.QuestStatus questState)
    {
        if (_images.ContainsKey(npcIndex) == false)
            return;

        switch (questState)
        {
            case Define.QuestStatus.None:
                _activeMarks[npcIndex] = false;
                break;
            case Define.QuestStatus.Can_Start:
                _images[npcIndex].sprite = _questSprites[Define.QuestStatus.Can_Start];
                _activeMarks[npcIndex] = true;
                break;
            case Define.QuestStatus.In_Progress:
                _images[npcIndex].sprite = _questSprites[Define.QuestStatus.In_Progress];
                _activeMarks[npcIndex] = true;
                break;
            case Define.QuestStatus.Can_Finish:
                _images[npcIndex].sprite = _questSprites[Define.QuestStatus.Can_Finish];
                _activeMarks[npcIndex] = true;
                break;
            case Define.QuestStatus.Finished:
                _activeMarks[npcIndex] = false;
                break;
        }
    }

    void UpdateQuestMarkPosition(int npcIndex)
    {
        Transform npc = Managers.NPC.GetNPC(npcIndex).transform;
        var rectTransform = _images[npcIndex].rectTransform;
        rectTransform.position = npc.position + Vector3.up * 2.5f;
    }

    void UpdateQuestMarkRotate(int npcIndex)
    {
        var rectTransform = _images[npcIndex].rectTransform;
        var playerPos = 2 * rectTransform.position - Camera.main.transform.position;
        rectTransform.LookAt(playerPos);
    }

    public void CancleToken(int npcIndex) { _cancleTokenSources[npcIndex]?.Cancel(); }
    public void DisposeToken(int npcIndex) { _cancleTokenSources[npcIndex]?.Dispose(); }

    void OnDestroy()
    {
        foreach (int key in _cancleTokenSources.Keys)
        {
            CancleToken(key);
            DisposeToken(key);
        }
    }
}
