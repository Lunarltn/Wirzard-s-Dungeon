using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager
{
    UI_TalkBubble _talkBubble;
    UI_NPCRecoverTime _nPCRecoverTime;
    Dictionary<int, Transform> _npcs = new Dictionary<int, Transform>();

    public void Init()
    {
        _talkBubble = Managers.UI.ShowSceneUI<UI_TalkBubble>();
        _nPCRecoverTime = Managers.UI.ShowSceneUI<UI_NPCRecoverTime>();
    }

    public void SetNPC(int idx, Transform npc)
    {
        _npcs.Add(idx, npc);
    }

    public Transform GetNPC(int idx)
    {
        if (_npcs.ContainsKey(idx))
            return _npcs[idx];
        return null;
    }

    public void ShowTalkBubble(int npcIndex, string contents, float flowTime)
    {
        _talkBubble.ShowTalkBubble(npcIndex, contents, flowTime);
    }

    public void ShowNPCRecoverTime(int npcIndex, float flowTime)
    {
        _nPCRecoverTime.ShowRecoverTime(npcIndex, flowTime);
    }

}
