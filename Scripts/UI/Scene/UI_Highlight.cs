using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Highlight : UI_Scene
{
    enum Images
    {
        Image_Clone
    }

    const float ShowTime = 0.8f;
    const float ScalePersent = 0.2f;
    Image _imageClone;
    List<CancellationTokenSource> _cancleTokenSources = new List<CancellationTokenSource>();
    List<Image> _clonePool = new List<Image>();

    public override bool Init()
    {
        BindImage(typeof(Images));

        GetImage(0).transform.gameObject.SetActive(false);
        _imageClone = GetImage(0);

        return true;
    }

    public void ShowHighlight(Transform target, float high)
    {
        int index = -1;
        for (int i = 0; i < _clonePool.Count; i++)
        {
            if (_clonePool[i].gameObject.activeSelf == false)
            {
                index = i;
                break;
            }
        }
        if (index == -1)
        {
            _clonePool.Add(Instantiate(_imageClone, transform));
            _cancleTokenSources.Add(new CancellationTokenSource());
            index = _clonePool.Count - 1;
        }
        AnimationShowHighlight(index, target, high).Forget();
    }

    async UniTask AnimationShowHighlight(int index, Transform target, float high)
    {
        _cancleTokenSources[index] = new CancellationTokenSource();
        _clonePool[index].gameObject.SetActive(true);
        float time = 0;
        while (time < ShowTime)
        {
            time += Time.deltaTime;
            _clonePool[index].rectTransform.position = target.position + Vector3.up * high;
            var playerPos = (target.position - Managers.Camera.MainCamera.transform.position).normalized;
            _clonePool[index].rectTransform.rotation = Quaternion.LookRotation(playerPos);
            if (time < ShowTime * ScalePersent)
                _clonePool[index].rectTransform.localScale =
                    (Vector2.one * 0.5f) + (Vector2.one * ((time / (ShowTime * ScalePersent)) * 0.3f));

            await UniTask.Yield(PlayerLoopTiming.Update, _cancleTokenSources[index].Token);
        }
        _clonePool[index].gameObject.SetActive(false);
    }

    public void CancleToken(int index) { _cancleTokenSources[index]?.Cancel(); }
    public void DisposeToken(int index) { _cancleTokenSources[index]?.Dispose(); }

    void OnDestroy()
    {
        for (int i = 0; i < _cancleTokenSources.Count; i++)
        {
            CancleToken(i);
            DisposeToken(i);
        }
    }
}
