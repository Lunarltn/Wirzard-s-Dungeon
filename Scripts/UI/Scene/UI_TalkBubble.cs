using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_TalkBubble : UI_Scene
{
    enum Images
    {
        Image_Clone
    }

    Dictionary<int, RectTransform> _rectTransforms = new Dictionary<int, RectTransform>();
    Dictionary<int, Image> _images = new Dictionary<int, Image>();
    Dictionary<int, TextMeshProUGUI> _texts = new Dictionary<int, TextMeshProUGUI>();
    Dictionary<int, CancellationTokenSource> _cancleTokenSources = new Dictionary<int, CancellationTokenSource>();

    const float OpAnimPlayTime = 0.5f;
    const float EdAnimPlayTime = 0.3f;

    public override bool Init()
    {
        BindImage(typeof(Images));

        GetImage(0).transform.gameObject.SetActive(false);

        foreach (int key in Managers.Data.NPCDic.Keys)
        {
            _rectTransforms.Add(key, Instantiate(GetImage(0).transform.gameObject, transform).GetComponent<RectTransform>());
            _images.Add(key, _rectTransforms[key].GetComponent<Image>());
            _texts.Add(key, _rectTransforms[key].GetChild(0).GetComponent<TextMeshProUGUI>());
            _cancleTokenSources.Add(key, new CancellationTokenSource());
        }

        return true;
    }

    public void ShowTalkBubble(int npcIndex, string contents, float flowTime)
    {
        _texts[npcIndex].text = contents;
        Transform npc = Managers.NPC.GetNPC(npcIndex).transform;
        AnimationShowTalkBubble(npc, npcIndex, flowTime).Forget();
    }

    async UniTask AnimationShowTalkBubble(Transform npc, int npcIndex, float flowTime)
    {
        if (_rectTransforms.ContainsKey(npcIndex) == false)
            return;

        if (_rectTransforms[npcIndex].gameObject.activeSelf)
        {
            CancleToken(npcIndex);
            _cancleTokenSources[npcIndex] = new CancellationTokenSource();
        }

        Color tempImageColor = Color.black;
        Color tempTextColor = Color.white;
        tempImageColor.a = 0;
        tempTextColor.a = 0;
        _images[npcIndex].color = tempImageColor;
        _texts[npcIndex].color = tempTextColor;
        _rectTransforms[npcIndex].gameObject.SetActive(true);
        _rectTransforms[npcIndex].localScale = new Vector3(0.3f, 1, 1);

        float timer = 0;
        while (timer < flowTime)
        {
            timer += Time.deltaTime;
            if (timer < OpAnimPlayTime)
            {
                _rectTransforms[npcIndex].localScale = new Vector3((timer / OpAnimPlayTime) * 0.7f + 0.3f, 1, 1);
                tempImageColor.a = timer / OpAnimPlayTime;
                tempTextColor.a = timer / OpAnimPlayTime;
                _images[npcIndex].color = tempImageColor;
                _texts[npcIndex].color = tempTextColor;
            }

            _rectTransforms[npcIndex].position = npc.position + Vector3.up * 2.5f;

            var playerPos = npc.position - Managers.Camera.MainCamera.transform.position;
            _rectTransforms[npcIndex].rotation = Quaternion.LookRotation(playerPos);

            if (timer > flowTime - EdAnimPlayTime)
            {
                tempImageColor.a = (flowTime - timer) / EdAnimPlayTime;
                tempTextColor.a = timer / OpAnimPlayTime;
                _images[npcIndex].color = tempImageColor;
                _texts[npcIndex].color = tempTextColor;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, _cancleTokenSources[npcIndex].Token);
        }

        _rectTransforms[npcIndex].gameObject.SetActive(false);
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
