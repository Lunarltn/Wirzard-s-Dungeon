using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_NPCRecoverTime : UI_Scene
{
    enum Images
    {
        Image_CloneBackground
    }

    Dictionary<int, RectTransform> _rectTransforms = new Dictionary<int, RectTransform>();
    Dictionary<int, Image> _images = new Dictionary<int, Image>();
    Dictionary<int, TextMeshProUGUI> _texts = new Dictionary<int, TextMeshProUGUI>();
    Dictionary<int, CancellationTokenSource> _cancleTokenSources = new Dictionary<int, CancellationTokenSource>();

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

    public void ShowRecoverTime(int npcIndex, float flowTime)
    {
        if (_rectTransforms.ContainsKey(npcIndex) == false)
            return;
        Transform npc = Managers.NPC.GetNPC(npcIndex).transform;
        AnimationShowRecoverTime(npc, npcIndex, flowTime).Forget();
    }

    async UniTask AnimationShowRecoverTime(Transform npc, int npcIndex, float flowTime)
    {
        if (_rectTransforms[npcIndex].gameObject.activeSelf)
        {
            CancleToken(npcIndex);
            _cancleTokenSources[npcIndex] = new CancellationTokenSource();
        }
        _rectTransforms[npcIndex].gameObject.SetActive(true);

        float timer = 0;
        while (timer < flowTime)
        {
            timer += Time.deltaTime;

            _images[npcIndex].fillAmount = timer / flowTime;
            _texts[npcIndex].text = string.Format("{0:00}", flowTime - timer);
            var playerPos = npc.position - Managers.Camera.MainCamera.transform.position;
            _rectTransforms[npcIndex].rotation = Quaternion.LookRotation(playerPos);
            _rectTransforms[npcIndex].position = npc.position + playerPos.normalized * -2 + Vector3.up;

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
