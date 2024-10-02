using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class AltarFlameBall : MonoBehaviour, IDetectionObject
{
    const float MIN_SCALE = 0.9f;
    const float MAX_SCALE = 1.0f;

    [SerializeField]
    GameObject _endEffect;
    MeshRenderer _renderer;

    CancellationTokenSource _cancelTokenSource;
    float currentScale = MAX_SCALE;
    bool isReverse = false;
    int _connectedCount;
    int maxIncomingConnections = 2;
    Color[] _colors = new Color[3]
    {new Color32(255,255,60,255),new Color32(255,120,0,255),new Color32(255,30,0,255)};
    public bool IsConnectedAll => _connectedCount == maxIncomingConnections;

    private void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
    }

    public void Init(int maxIncomingConnections)
    {
        this.maxIncomingConnections = maxIncomingConnections;

        ChangeColor(_colors[2 - maxIncomingConnections]);
        _connectedCount = 0;
        _endEffect.SetActive(false);
        Cancel();
        FadeIn(1).Forget();
    }

    void Update()
    {
        if (isReverse == false)
        {
            currentScale -= Time.deltaTime * 0.3f;
            if (currentScale < MIN_SCALE)
                isReverse = true;
        }
        else
        {
            currentScale += Time.deltaTime * 0.3f;
            if (currentScale > MAX_SCALE)
                isReverse = false;
        }
        transform.localScale = Vector3.one * currentScale;
    }

    public void Disable()
    {
        _endEffect.SetActive(true);
        FadeOut(1).Forget();
    }

    public void Exit()
    {
        _connectedCount = 0;
        ChangeColor(_colors[2 - maxIncomingConnections]);
    }

    public void Reaction()
    {
        if (_connectedCount >= maxIncomingConnections)
            return;
        _connectedCount++;

        ChangeColor(_colors[2 + _connectedCount - maxIncomingConnections]);
    }

    void ChangeColor(Color color)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(mpb);
        mpb.SetColor("_TintColor", color);
        mpb.SetColor("_RimColor", color);
        _renderer.SetPropertyBlock(mpb);
    }

    async UniTask FadeIn(float second)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(mpb);
        _cancelTokenSource = new CancellationTokenSource();

        float timer = 0;
        while (timer < second)
        {
            timer += Time.deltaTime;
            mpb.SetFloat("_MaskCutOut", timer / second);
            _renderer.SetPropertyBlock(mpb);
            await UniTask.NextFrame(_cancelTokenSource.Token);
        }
    }


    async UniTask FadeOut(float second)
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        _renderer.GetPropertyBlock(mpb);
        _cancelTokenSource = new CancellationTokenSource();

        float timer = 0;
        while (timer < second)
        {
            timer += Time.deltaTime;
            mpb.SetFloat("_MaskCutOut", 1 - timer / second);
            _renderer.SetPropertyBlock(mpb);
            await UniTask.NextFrame(_cancelTokenSource.Token);
        }
        Managers.Resource.Destroy(gameObject);

    }

    void Cancel()
    {
        _cancelTokenSource?.Cancel();
    }

    void Dispose()
    {
        _cancelTokenSource?.Dispose();
    }


    private void OnDisable()
    {
        Cancel();
    }

    private void OnDestroy()
    {
        Cancel();
        Dispose();
    }
}
