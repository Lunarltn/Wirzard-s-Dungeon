using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class AltarStone : MonoBehaviour, IInteraction
{
    [SerializeField]
    AltarFlame _altarFlame;

    public bool IsReversed;

    float _minAngle = 0;
    float _maxAngle = 180;
    float _unitAngle = 45;
    float currentAngle;
    bool _isReverse;

    private void Start()
    {
        if (IsReversed)
        {
            _minAngle = -180;
            _maxAngle = 0;
            _isReverse = true;
        }

        int rand = Random.Range(0, 5);
        if (_isReverse)
        {
            currentAngle = rand * -_unitAngle;
            if (currentAngle <= _minAngle)
                _isReverse = false;
        }
        else
        {
            currentAngle = rand * _unitAngle;
            if (currentAngle >= _maxAngle)
                _isReverse = true;
        }

        _altarFlame.RingCenter.localRotation = Quaternion.Euler(0, currentAngle, 0);

    }

    public string GetInterectionMessage()
    {
        return "회전시키기";
    }

    public string GetInterectionName()
    {
        return "제단";
    }

    public void Interect()
    {
        if (_isReverse)
        {
            currentAngle -= _unitAngle;
            if (currentAngle <= _minAngle)
                _isReverse = false;
        }
        else
        {
            currentAngle += _unitAngle;
            if (currentAngle >= _maxAngle)
                _isReverse = true;
        }
        _altarFlame.RingCenter.DOLocalRotate(Vector3.up * currentAngle, 1);
    }

    public void SetPosition(Vector3 position)
    {
    }
}
