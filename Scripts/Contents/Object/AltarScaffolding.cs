using Cysharp.Threading;
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltarScaffolding : MonoBehaviour, IInteraction
{
    [SerializeField]
    QuizRoom _room;
    [SerializeField]
    LineRenderer _laser;
    [SerializeField]
    GameObject[] _circles = new GameObject[2];
    const float LASER_MAX_DISTANCE = 15;
    int _detectionObjectMask;
    bool _isUsing;
    bool _isCompleteDelay;

    IDetectionObject _detectionObject;

    private void Start()
    {
        _detectionObjectMask = Managers.Layer.DetectionLayerMask;
        _laser.gameObject.SetActive(false);
        _circles[0].SetActive(false);
        _circles[1].SetActive(false);
    }

    async UniTask WaitComplete()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
        if (_room.IsCompletedQuiz())
        {
            _detectionObject?.Exit();
            _detectionObject = null;
            _laser.gameObject.SetActive(false);
            _room.NextRound(1f).Forget();
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
        }

        _isCompleteDelay = true;
    }

    public string GetInterectionMessage()
    {
        if (_isUsing)
            return "��Ȱ��ȭ";
        else
            return "Ȱ��ȭ";
    }

    public string GetInterectionName()
    {
        return "����";
    }

    public void Interect()
    {
        if (_isUsing == false)
        {
            _isUsing = true;
            _circles[0].SetActive(true);
            _circles[1].SetActive(true);
            Managers.PlayerInfo.Controller.CommandMoveToDestinationState(transform.position, -transform.forward);
            Managers.Camera.OnEnableCinemachineViewTopAngle(_room.CameraFollow, new Vector3(90, 90, 0));
            ShotRay();
            WaitComplete().Forget();
        }
        else
        {
            if (_isCompleteDelay == false)
                return;
            _isUsing = false;
            _circles[0].SetActive(false);
            _circles[1].SetActive(false);
            Managers.PlayerInfo.Controller.ResetState();
            Managers.Camera.OnDisableCinemachineViewTopAngle();
            if (_detectionObject != null)
            {
                _detectionObject.Exit();
                _detectionObject = null;
                _laser.gameObject.SetActive(false);
            }
            _isCompleteDelay = false;
        }
    }

    public void SetPosition(Vector3 position)
    {

    }

    void ShotRay()
    {
        _laser.gameObject.SetActive(true);
        if (Physics.Raycast(_laser.transform.position, _laser.transform.forward, out RaycastHit hit, LASER_MAX_DISTANCE, _detectionObjectMask))
        {
            _detectionObject = Util.FindParent<IDetectionObject>(hit.transform.gameObject);
            _detectionObject?.Reaction();
            _laser.SetPosition(1, Vector3.forward * (hit.distance + 0.5f));
        }
    }
}