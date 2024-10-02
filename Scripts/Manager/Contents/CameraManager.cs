using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

public class CameraManager
{
    Camera _mainCamera;
    public Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            return _mainCamera;
        }
    }
    protected const string PLAYER = "Player";
    protected const string SKILL = "Skill";

    const float ZOOM_IN_FOV = 25f;
    const float ZOOM_OUT_FOV = 40f;
    const float MAX_AIM_DISTANCE = 50f;
    const float MAX_CAMERA_X_SENSITIVITY = 500f;
    const float MAX_CAMERA_Y_SENSITIVITY = 4f;

    int _initCullingMask;
    float _maxAimDistance = 50f;
    float _minAimDistance = 4f;
    int _cameraSensitivity;
    RaycastHit _hitAimPointRay;
    CancellationTokenSource _zoomCancleTokenSource;
    CancellationTokenSource _shakeCancleTokenSource;
    CinemachineFreeLook _cinemachine_freeLook;

    public RaycastHit HitAimPointRay => _hitAimPointRay;
    public float FreeLookYAxis => _cinemachine_freeLook.m_YAxis.Value * -2 + 1;
    public Vector3 CameraDir
    {
        get
        {
            Vector3 dir = MainCamera.transform.localRotation * Vector3.forward;
            return new Vector3(dir.x, 0, dir.z).normalized;
        }
    }

    public int CameraSensitivity
    {
        get { return _cameraSensitivity; }
        set
        {
            _cameraSensitivity = value;
            RefreshCameraSensitivity(_cameraSensitivity);
        }
    }

    public Transform DetectAimPointRayTarget()
    {
        int _exceptionLayer = Managers.Layer.PlayerLayerMask | Managers.Layer.SkillLayerMask;
        Ray ray = MainCamera.ViewportPointToRay(Vector2.one * 0.5f);
        Transform target = null;
        if (Physics.Raycast(ray, out _hitAimPointRay, _maxAimDistance, ~_exceptionLayer, QueryTriggerInteraction.Ignore)
            && _hitAimPointRay.distance > _minAimDistance)
            target = _hitAimPointRay.transform;

        return target;
    }

    public Vector3 DetectAimPointRayPosition()
    {
        int _exceptionLayer = Managers.Layer.PlayerLayerMask | Managers.Layer.SkillLayerMask;
        Ray ray = MainCamera.ViewportPointToRay(Vector2.one * 0.5f);
        Vector3 targetPoint;
        if (Physics.Raycast(ray, out _hitAimPointRay, _maxAimDistance, ~_exceptionLayer, QueryTriggerInteraction.Ignore)
            && _hitAimPointRay.distance > _minAimDistance)
            targetPoint = _hitAimPointRay.point;
        else
            targetPoint = ray.origin + ray.direction * _maxAimDistance;

        return targetPoint;
    }

    readonly CinemachineFreeLook.Orbit[] _freeLockOrbit = new CinemachineFreeLook.Orbit[3]
        {
            new CinemachineFreeLook.Orbit()
        {
            m_Height = 5.5f,
            m_Radius = 3.8f
        },
            new CinemachineFreeLook.Orbit()
        {
            m_Height = 0.34f,
            m_Radius = 6.6f
        },new CinemachineFreeLook.Orbit()
        {
            m_Height = -4f,
            m_Radius = 3.38f
        }
        };

    CinemachineVirtualCamera _cinemachine_VirtualCamera_ViewNPC;
    CinemachineVirtualCamera _cinemachine_VirtualCamera_ViewTopAngle;
    CinemachineBasicMultiChannelPerlin[] _rigMCs = new CinemachineBasicMultiChannelPerlin[3];

    public void Init()
    {
        InitCinemachineFreeLook();
        InitCinemachineViewNPC();
        InitCinemachineViewTopAngle();

        _initCullingMask = MainCamera.cullingMask;
        CameraSensitivity = 50;
    }

    void InitCinemachineFreeLook()
    {
        _cinemachine_freeLook = GameObject.FindObjectOfType<CinemachineFreeLook>();
        if (_cinemachine_freeLook == null)
            _cinemachine_freeLook = Managers.Resource.Instantiate("Camera/Cinemachine_FreeLook").GetComponent<CinemachineFreeLook>(); ;
        _cinemachine_freeLook.m_Orbits = _freeLockOrbit;

        for (int i = 0; i < 3; i++)
        {
            _rigMCs[i] = _cinemachine_freeLook.GetRig(i).GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    void InitCinemachineViewNPC()
    {
        var cinemachine_ViewNPC = Managers.Resource.Instantiate("Camera/Cinemachine_ViewNPC");
        _cinemachine_VirtualCamera_ViewNPC = cinemachine_ViewNPC.GetComponent<CinemachineVirtualCamera>();
        cinemachine_ViewNPC.SetActive(false);
    }

    void InitCinemachineViewTopAngle()
    {
        var cinemachine_ViewTopAngle = Managers.Resource.Instantiate("Camera/Cinemachine_ViewTopAngle");
        _cinemachine_VirtualCamera_ViewTopAngle = cinemachine_ViewTopAngle.GetComponent<CinemachineVirtualCamera>();
        cinemachine_ViewTopAngle.SetActive(false);
    }
    public void OnEnableCinemachineViewNPC(Transform follow, Transform lookAt)
    {
        _cinemachine_VirtualCamera_ViewNPC.Follow = follow;
        _cinemachine_VirtualCamera_ViewNPC.LookAt = lookAt;
        MainCamera.cullingMask = ~(Managers.Layer.PlayerLayerMask | Managers.Layer.MiniMapLayerMask);
        _cinemachine_VirtualCamera_ViewNPC.gameObject.SetActive(true);
    }

    public void OnDisableCinemachineViewNPC()
    {
        MainCamera.cullingMask = _initCullingMask;
        _cinemachine_VirtualCamera_ViewNPC.gameObject.SetActive(false);
    }

    public void OnEnableCinemachineViewTopAngle(Transform follow, Vector3 angle)
    {
        _cinemachine_VirtualCamera_ViewTopAngle.Follow = follow;
        _cinemachine_VirtualCamera_ViewTopAngle.transform.rotation = Quaternion.Euler(angle);
        _cinemachine_VirtualCamera_ViewTopAngle.gameObject.SetActive(true);
    }

    public void OnDisableCinemachineViewTopAngle()
    {
        _cinemachine_VirtualCamera_ViewTopAngle.gameObject.SetActive(false);
    }

    void RefreshCameraSensitivity(float sensitivity)
    {
        _cinemachine_freeLook.m_YAxis.m_MaxSpeed = MAX_CAMERA_Y_SENSITIVITY * sensitivity * 0.01f;
        _cinemachine_freeLook.m_XAxis.m_MaxSpeed = MAX_CAMERA_X_SENSITIVITY * sensitivity * 0.01f;
    }

    void StopCamera()
    {
        _cinemachine_freeLook.m_YAxis.m_MaxSpeed = 0;
        _cinemachine_freeLook.m_XAxis.m_MaxSpeed = 0;
    }

    public void CameraLock()
    {
        StopCamera();
    }

    public void CameraUnLock()
    {
        RefreshCameraSensitivity(_cameraSensitivity);
    }

    public void SetMaxAimDistance(float value = MAX_AIM_DISTANCE)
    {
        _maxAimDistance = value;
    }

    public void ZoomInCamera()
    {
        AnimationZoomInCamera().Forget();
    }

    public void ZoomOutCamera()
    {
        AnimationZoomOutCamera().Forget();
    }

    public void PlayShake(float shakePower, float shakeTime)
    {
        StartSkake(shakePower, shakeTime).Forget();
    }

    async UniTask StartSkake(float shakePower, float shakeTime)
    {
        Shake(1.5f, shakePower);
        CancleToken(_shakeCancleTokenSource);
        _shakeCancleTokenSource = new CancellationTokenSource();
        await UniTask.Delay(TimeSpan.FromSeconds(shakeTime), delayTiming: PlayerLoopTiming.FixedUpdate, cancellationToken: _shakeCancleTokenSource.Token);
        Shake(0, 0);
    }

    private void Shake(float amplitudeGain, float frequencyGain)
    {
        for (int i = 0; i < 3; i++)
            _rigMCs[i].m_AmplitudeGain = amplitudeGain;

        for (int i = 0; i < 3; i++)
            _rigMCs[i].m_FrequencyGain = frequencyGain;
    }

    async UniTask AnimationZoomInCamera()
    {
        CancleToken(_zoomCancleTokenSource);
        _zoomCancleTokenSource = new CancellationTokenSource();
        float currentFOV = _cinemachine_freeLook.m_Lens.FieldOfView;
        float fovValue = ZOOM_IN_FOV - currentFOV;
        float lerpTime = 0.5f;
        float timer = 0;
        while (lerpTime > timer)
        {
            timer += Time.deltaTime;
            float per = (timer / lerpTime);
            _cinemachine_freeLook.m_Lens.FieldOfView = currentFOV + fovValue * per;
            await UniTask.WaitForFixedUpdate(_zoomCancleTokenSource.Token);
        }
    }

    async UniTask AnimationZoomOutCamera()
    {
        CancleToken(_zoomCancleTokenSource);
        _zoomCancleTokenSource = new CancellationTokenSource();
        float currentFOV = _cinemachine_freeLook.m_Lens.FieldOfView;
        float fovValue = ZOOM_OUT_FOV - currentFOV;
        float lerpTime = 0.5f;
        float timer = 0;
        while (lerpTime > timer)
        {
            timer += Time.deltaTime;
            float per = (timer / lerpTime);
            _cinemachine_freeLook.m_Lens.FieldOfView = currentFOV + fovValue * per;
            await UniTask.WaitForFixedUpdate(_zoomCancleTokenSource.Token);
        }
    }

    public void CancleToken(CancellationTokenSource cancelTokenSource)
    {
        cancelTokenSource?.Cancel();
    }
    public void DisposeToken(CancellationTokenSource cancelTokenSource)
    {
        cancelTokenSource?.Dispose();
    }

    public void OnDisable()
    {
        CancleToken(_zoomCancleTokenSource);
        CancleToken(_shakeCancleTokenSource);

        DisposeToken(_zoomCancleTokenSource);
        DisposeToken(_shakeCancleTokenSource);
    }
}

