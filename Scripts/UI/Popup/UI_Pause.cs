using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;
using static Define;

public class UI_Pause : UI_Popup
{
    #region enum
    enum Texts
    {
        Text_Resume,
        Text_Options,
        Text_Exit,
        Text_Save
    }

    enum Images
    {
        Image_PausePanel,
        Image_OptionsPanel,
        Image_Graphic,
        Image_Audio,
        Image_Mouse
    }

    enum GameObjects
    {
        _GraphicGroup,
        _AudioGroup,
        _MouseGroup,
        _GraphicDisplay,
        _GraphicFullScreen,
        _AudioMaster,
        _AudioMusic,
        _AudioSoundFX,
        _MouseSensitivity,
        _MouseAimingLine,
        _MouseAimingLineAlpha
    }

    enum UIComponents
    {
        Dropdown,
        Toggle,
        Slider,
        Text_Value,
        Image_Preview
    }
    #endregion
    Sequence _currentSequence;
    Color32 _optionsPanelColor;
    readonly Color32 _optionsButtonColor = new Color32(29, 29, 29, 255);
    int _currentOptionsMenuIndex;

    #region options
    bool _isChangedOptions;

    struct DisplaySetting
    {
        int _resolutionIndex;
        public int resolutionIndex
        {
            get { return _resolutionIndex; }
            set
            {
                if (_resolutionIndex == value) return;
                _resolutionIndex = value;
                ChangeOptions?.Invoke();
            }
        }
        FullScreenMode _screenMode;
        public FullScreenMode screenMode
        {
            get { return _screenMode; }
            set
            {
                if (_screenMode == value) return;
                _screenMode = value;
                ChangeOptions?.Invoke();
            }
        }
        public Action ChangeOptions;

        public bool Equals(DisplaySetting displaySetting)
        {
            return resolutionIndex == displaySetting.resolutionIndex
           && screenMode == displaySetting.screenMode;
        }
    }
    List<UnityEngine.Resolution> _resolutions = new List<UnityEngine.Resolution>();
    Dropdown _resolutionDropdown;
    Toggle _screenToggle;
    DisplaySetting _beforeChangeDisplaySetting;
    DisplaySetting _currentDisplaySetting;

    struct AudioSetting
    {
        int _master;
        public int master
        {
            get { return _master; }
            set
            {
                if (_master == value) return;
                _master = value;
                ChangeOptions?.Invoke();
            }
        }
        int _music;
        public int music
        {
            get { return _music; }
            set
            {
                if (_music == value) return;
                _music = value;
                ChangeOptions?.Invoke();
            }
        }
        int _soundFX;
        public int soundFX
        {
            get { return _soundFX; }
            set
            {
                if (_soundFX == value) return;
                _soundFX = value;
                ChangeOptions?.Invoke();
            }
        }
        public Action ChangeOptions;

        public bool Equals(AudioSetting audioSetting)
        {
            return master == audioSetting.master
           && music == audioSetting.music
           && soundFX == audioSetting.soundFX;
        }
    }
    Slider _masterSlider;
    TextMeshProUGUI _masterValue;
    Slider _musicSlider;
    TextMeshProUGUI _musicValue;
    Slider _soundFXSlider;
    TextMeshProUGUI _soundFXValue;
    AudioSetting _beforeChangeAudioSetting;
    AudioSetting _currentAudioSetting;

    struct MouseSetting
    {
        int _sensitivity;
        public int sensitivity
        {
            get { return _sensitivity; }
            set
            {
                if (_sensitivity == value) return;
                _sensitivity = value;
                ChangeOptions?.Invoke();
            }
        }
        int _aimingLineIndex;
        public int aimingLineIndex
        {
            get { return _aimingLineIndex; }
            set
            {
                if (_aimingLineIndex == value) return;
                _aimingLineIndex = value;
                ChangeOptions?.Invoke();
            }
        }
        float _aimingLineAlpha;
        public float aimingLineAlpha
        {
            get { return _aimingLineAlpha; }
            set
            {
                if (_aimingLineAlpha == value) return;
                _aimingLineAlpha = value;
                ChangeOptions?.Invoke();
            }
        }
        public Action ChangeOptions;

        public bool Equals(MouseSetting mouseSetting)
        {
            return sensitivity == mouseSetting.sensitivity
           && aimingLineIndex == mouseSetting.aimingLineIndex
           && aimingLineAlpha == mouseSetting.aimingLineAlpha;
        }
    }
    Slider _sensitivitySlider;
    TextMeshProUGUI _sensitivityValue;
    Dropdown _aimingLineDropdown;
    Image _aimingLinePreview;
    Slider _aimingLineAlphaSlider;
    TextMeshProUGUI _aimingLineAlphaValue;
    MouseSetting _beforeChangeMouseSetting;
    MouseSetting _currentMouseSetting;
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        //Bind
        BindTMP(typeof(Texts));
        BindImage(typeof(Images));
        BindObject(typeof(GameObjects));

        //Color
        _optionsPanelColor = GetImage((int)Images.Image_OptionsPanel).color;

        //PausePanel
        BindEvent(GetTMP((int)Texts.Text_Resume).gameObject, EnterResumeButton, Define.UI_Event.Enter);
        BindEvent(GetTMP((int)Texts.Text_Options).gameObject, EnterOptionsButton, Define.UI_Event.Enter);
        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, EnterExitButton, Define.UI_Event.Enter);

        BindEvent(GetTMP((int)Texts.Text_Resume).gameObject, ExitButton, Define.UI_Event.Exit);
        BindEvent(GetTMP((int)Texts.Text_Options).gameObject, ExitButton, Define.UI_Event.Exit);
        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, ExitButton, Define.UI_Event.Exit);

        BindEvent(GetTMP((int)Texts.Text_Resume).gameObject, ClickResumeButton, Define.UI_Event.Click);
        BindEvent(GetTMP((int)Texts.Text_Options).gameObject, ClickOptionsButton, Define.UI_Event.Click);
        BindEvent(GetTMP((int)Texts.Text_Exit).gameObject, ClickExitButton, Define.UI_Event.Click);

        //OptionsPanel
        BindEvent(GetImage((int)Images.Image_Graphic).gameObject, EnterOptionsMenuButton, Define.UI_Event.Enter);
        BindEvent(GetImage((int)Images.Image_Audio).gameObject, EnterOptionsMenuButton, Define.UI_Event.Enter);
        BindEvent(GetImage((int)Images.Image_Mouse).gameObject, EnterOptionsMenuButton, Define.UI_Event.Enter);

        BindEvent(GetImage((int)Images.Image_Graphic).gameObject, ExitButton, Define.UI_Event.Exit);
        BindEvent(GetImage((int)Images.Image_Audio).gameObject, ExitButton, Define.UI_Event.Exit);
        BindEvent(GetImage((int)Images.Image_Mouse).gameObject, ExitButton, Define.UI_Event.Exit);

        BindEvent(GetImage((int)Images.Image_Graphic).gameObject, ClickOptionsMenuButton, Define.UI_Event.Click);
        BindEvent(GetImage((int)Images.Image_Audio).gameObject, ClickOptionsMenuButton, Define.UI_Event.Click);
        BindEvent(GetImage((int)Images.Image_Mouse).gameObject, ClickOptionsMenuButton, Define.UI_Event.Click);

        BindEvent(GetTMP((int)Texts.Text_Save).gameObject, ClickSaveButton, Define.UI_Event.Click);

        _beforeChangeDisplaySetting.ChangeOptions += ChangeOptions;
        _beforeChangeAudioSetting.ChangeOptions += ChangeOptions;
        _beforeChangeMouseSetting.ChangeOptions += ChangeOptions;

        InitResolutionDropdown();
        InitScreenModeToggle();

        InitMasterSlider();
        InitMusicSlider();
        InitSoundFXSlider();

        InitSensitivitySlider();
        InitAimingLineDropdown();
        InitAimingLineAlphaSlider();



        return true;
    }

    protected override void OnEnable()
    {
        //Not Sort
        //base.OnEnable();
        Time.timeScale = 0;

        GetImage((int)Images.Image_PausePanel).gameObject.SetActive(true);
        GetImage((int)Images.Image_OptionsPanel).gameObject.SetActive(false);

        _currentOptionsMenuIndex = 0;

        int optionMenuStartIndex = (int)Images.Image_OptionsPanel + 1;
        for (int i = 0; i < Enum.GetValues(typeof(Images)).Length - optionMenuStartIndex; i++)
        {
            GetImage(i + optionMenuStartIndex).color = _currentOptionsMenuIndex == i ? _optionsButtonColor : _optionsPanelColor;
            GetObject(i).SetActive(_currentOptionsMenuIndex == i);
        }

        RefreshOptionsUI();
    }

    void RefreshOptionsUI()
    {
        _resolutionDropdown.value = _currentDisplaySetting.resolutionIndex;
        _screenToggle.isOn = _currentDisplaySetting.screenMode == FullScreenMode.FullScreenWindow;

        _masterSlider.value = _currentAudioSetting.master;
        _masterValue.text = _currentAudioSetting.master.ToString();
        _musicSlider.value = _currentAudioSetting.music;
        _musicValue.text = _currentAudioSetting.music.ToString();
        _soundFXSlider.value = _currentAudioSetting.soundFX;
        _soundFXValue.text = _currentAudioSetting.soundFX.ToString();

        _sensitivitySlider.value = _currentMouseSetting.sensitivity;
        _sensitivityValue.text = _currentMouseSetting.sensitivity.ToString();
        _aimingLineDropdown.value = _currentMouseSetting.aimingLineIndex;
        _aimingLinePreview.sprite = Managers.HotKey.AimingLineSprites[_currentMouseSetting.aimingLineIndex];
        _aimingLineAlphaSlider.value = _currentMouseSetting.aimingLineAlpha * 10;
        _aimingLineAlphaValue.text = _currentMouseSetting.aimingLineAlpha.ToString("0.0");

        _isChangedOptions = false;
        GetTMP((int)Texts.Text_Save).color = Color.gray;
    }

    void OnDisable()
    {
        _currentSequence?.Kill();
    }

    #region DisplaySetting
    void ChangeResolutionDropdown(int index)
    {
        _beforeChangeDisplaySetting.resolutionIndex = index;
    }

    void InitResolutionDropdown()
    {
        _resolutionDropdown = Util.FindChild<Dropdown>(GetObject((int)GameObjects._GraphicDisplay), GetName(UIComponents.Dropdown), true);
        _resolutionDropdown.onValueChanged.AddListener(ChangeResolutionDropdown);

        _resolutions.AddRange(Screen.resolutions);
        _resolutionDropdown.options.Clear();

        int optionNum = 0;
        foreach (UnityEngine.Resolution item in _resolutions)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = item.width + "x" + item.height + " " + item.refreshRate + "hz";
            _resolutionDropdown.options.Add(option);

            if (item.width == Screen.width && item.height == Screen.height)
            {
                _resolutionDropdown.value = optionNum;
                _currentDisplaySetting.resolutionIndex = optionNum;
                ChangeResolutionDropdown(_currentDisplaySetting.resolutionIndex);
            }
            optionNum++;
        }
        _resolutionDropdown.RefreshShownValue();
    }

    void ChangeScreenModeToggle(bool isFull)
    {
        _beforeChangeDisplaySetting.screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    void InitScreenModeToggle()
    {
        _screenToggle = Util.FindChild<Toggle>(GetObject((int)GameObjects._GraphicFullScreen), GetName(UIComponents.Toggle), true);
        _screenToggle.onValueChanged.AddListener(ChangeScreenModeToggle);
        _screenToggle.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow);
        _currentDisplaySetting.screenMode = Screen.fullScreenMode;
        ChangeScreenModeToggle(_currentDisplaySetting.screenMode == FullScreenMode.FullScreenWindow);
    }
    #endregion

    #region AudioSetting
    void ChangeMasterSlider(float value)
    {
        _masterValue.text = value.ToString();
        _beforeChangeAudioSetting.master = (int)value;
    }

    void InitMasterSlider()
    {
        _masterSlider = Util.FindChild<Slider>(GetObject((int)GameObjects._AudioMaster), GetName(UIComponents.Slider), true);
        _masterValue = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._AudioMaster), GetName(UIComponents.Text_Value), true);
        _masterSlider.onValueChanged.AddListener(ChangeMasterSlider);
        _masterSlider.value = 100;
        _currentAudioSetting.master = 100;
        ChangeMasterSlider(_currentAudioSetting.master);
    }

    void ChangeMusicSlider(float value)
    {
        _musicValue.text = value.ToString();
        _beforeChangeAudioSetting.music = (int)value;
    }

    void InitMusicSlider()
    {
        _musicSlider = Util.FindChild<Slider>(GetObject((int)GameObjects._AudioMusic), GetName(UIComponents.Slider), true);
        _musicValue = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._AudioMusic), GetName(UIComponents.Text_Value), true);
        _musicSlider.onValueChanged.AddListener(ChangeMusicSlider);
        _musicSlider.value = 100;
        _currentAudioSetting.music = 100;
        ChangeMusicSlider(_currentAudioSetting.music);
    }

    void ChangeSoundFXSlider(float value)
    {
        _soundFXValue.text = value.ToString();
        _beforeChangeAudioSetting.soundFX = (int)value;
    }

    void InitSoundFXSlider()
    {
        _soundFXSlider = Util.FindChild<Slider>(GetObject((int)GameObjects._AudioSoundFX), GetName(UIComponents.Slider), true);
        _soundFXValue = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._AudioSoundFX), GetName(UIComponents.Text_Value), true);
        _soundFXSlider.onValueChanged.AddListener(ChangeSoundFXSlider);
        _soundFXSlider.value = 100;
        _currentAudioSetting.soundFX = 100;
        ChangeSoundFXSlider(_currentAudioSetting.soundFX);
    }
    #endregion

    #region MouseSetting
    void ChangeSensitivitySlider(float value)
    {
        _sensitivityValue.text = value.ToString();
        _beforeChangeMouseSetting.sensitivity = (int)value;
    }

    void InitSensitivitySlider()
    {
        _sensitivitySlider = Util.FindChild<Slider>(GetObject((int)GameObjects._MouseSensitivity), GetName(UIComponents.Slider), true);
        _sensitivityValue = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._MouseSensitivity), GetName(UIComponents.Text_Value), true);
        _sensitivitySlider.onValueChanged.AddListener(ChangeSensitivitySlider);
        _sensitivitySlider.value = Managers.Camera.CameraSensitivity;
        _currentMouseSetting.sensitivity = Managers.Camera.CameraSensitivity;
        ChangeSensitivitySlider(_currentMouseSetting.sensitivity);
    }

    void ChangeAimingLineDropdown(int index)
    {
        _aimingLinePreview.sprite = Managers.HotKey.AimingLineSprites[index];
        _beforeChangeMouseSetting.aimingLineIndex = index;
    }

    void InitAimingLineDropdown()
    {
        _aimingLineDropdown = Util.FindChild<Dropdown>(GetObject((int)GameObjects._MouseAimingLine), GetName(UIComponents.Dropdown), true);
        _aimingLinePreview = Util.FindChild<Image>(GetObject((int)GameObjects._MouseAimingLine), GetName(UIComponents.Image_Preview), true);
        _aimingLineDropdown.onValueChanged.AddListener(ChangeAimingLineDropdown);

        _aimingLineDropdown.options.Clear();

        foreach (Sprite item in Managers.HotKey.AimingLineSprites)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = item.name;
            _aimingLineDropdown.options.Add(option);

            _aimingLineDropdown.value = Managers.HotKey.CurrentAimingLineIndex;
            _currentMouseSetting.aimingLineIndex = Managers.HotKey.CurrentAimingLineIndex;
            ChangeAimingLineDropdown(_currentMouseSetting.aimingLineIndex);
        }
        _resolutionDropdown.RefreshShownValue();
    }

    void ChangeAimingLineAlphaSlider(float value)
    {
        float alpha = value * 0.1f;
        Color color = _aimingLinePreview.color;
        color.a = alpha;
        _aimingLinePreview.color = color;
        _aimingLineAlphaValue.text = alpha.ToString("0.0");
        _beforeChangeMouseSetting.aimingLineAlpha = alpha;
    }

    void InitAimingLineAlphaSlider()
    {
        _aimingLineAlphaSlider = Util.FindChild<Slider>(GetObject((int)GameObjects._MouseAimingLineAlpha), GetName(UIComponents.Slider), true);
        _aimingLineAlphaValue = Util.FindChild<TextMeshProUGUI>(GetObject((int)GameObjects._MouseAimingLineAlpha), GetName(UIComponents.Text_Value), true);
        _aimingLineAlphaSlider.onValueChanged.AddListener(ChangeAimingLineAlphaSlider);
        _aimingLineAlphaSlider.value = Managers.HotKey.CurrentAimingLineAlpha * 10;
        _currentMouseSetting.aimingLineAlpha = Managers.HotKey.CurrentAimingLineAlpha;
        ChangeAimingLineAlphaSlider(_currentMouseSetting.aimingLineAlpha);
    }
    #endregion

    #region PauseMenu
    void EnterResumeButton(PointerEventData evt)
    {
        AnimationSizeUpText(GetTMP((int)Texts.Text_Resume));
    }

    void EnterOptionsButton(PointerEventData evt)
    {
        AnimationSizeUpText(GetTMP((int)Texts.Text_Options));
    }

    void EnterExitButton(PointerEventData evt)
    {
        AnimationSizeUpText(GetTMP((int)Texts.Text_Exit));
    }

    void ClickResumeButton(PointerEventData evt)
    {
        ClosePopupUI<UI_Pause>(KeyCode.Escape);
    }

    void ClickOptionsButton(PointerEventData evt)
    {
        GetImage((int)Images.Image_PausePanel).gameObject.SetActive(false);
        GetImage((int)Images.Image_OptionsPanel).gameObject.SetActive(true);
    }

    void ClickExitButton(PointerEventData evt)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    void AnimationSizeUpText(TextMeshProUGUI tmp)
    {
        _currentSequence?.Kill();

        _currentSequence = DOTween.Sequence()
        .OnStart(() =>
        {
            tmp.fontSize = 50;
        })
        .SetAutoKill(false)
        .SetUpdate(true)
        .Append(DOTween.To(() => tmp.fontSize, x => tmp.fontSize = x, 60, 0.5f))
        .OnKill(() =>
        {
            tmp.fontSize = 50;
        });
    }
    #endregion

    #region OptionsMenu
    void EnterOptionsMenuButton(PointerEventData evt)
    {
        int optionMenuStartIndex = (int)Images.Image_OptionsPanel + 1;
        for (int i = 0; i < Enum.GetValues(typeof(Images)).Length - optionMenuStartIndex; i++)
        {
            if (ReferenceEquals(evt.pointerEnter, GetImage(i + optionMenuStartIndex).gameObject))
            {
                if (_currentOptionsMenuIndex == i)
                    return;

                AnimationChangeImageColor(GetImage(i + optionMenuStartIndex), i, _optionsPanelColor, _optionsButtonColor);
                break;
            }
        }
    }

    void ClickOptionsMenuButton(PointerEventData evt)
    {
        int optionMenuStartIndex = (int)Images.Image_OptionsPanel + 1;
        for (int i = 0; i < Enum.GetValues(typeof(Images)).Length - optionMenuStartIndex; i++)
        {
            if (ReferenceEquals(evt.pointerEnter, GetImage(i + optionMenuStartIndex).gameObject))
            {
                if (_currentOptionsMenuIndex == i)
                    return;
                _currentOptionsMenuIndex = i;

                for (int j = 0; j < Enum.GetValues(typeof(Images)).Length - optionMenuStartIndex; j++)
                {
                    GetImage(j + optionMenuStartIndex).color = j == i ? _optionsButtonColor : _optionsPanelColor;
                    GetObject(j).SetActive(j == i);
                }
                break;
            }
        }
    }

    void AnimationChangeImageColor(Image image, int index, Color beforeColor, Color afterColor)
    {
        _currentSequence?.Kill();

        _currentSequence = DOTween.Sequence()
        .OnStart(() =>
        {
            image.color = beforeColor;
        })
        .SetAutoKill(false)
        .SetUpdate(true)
        .Append(image.DOColor(afterColor, 0.5f))
        .OnKill(() =>
        {
            if (index != _currentOptionsMenuIndex)
                image.color = beforeColor;
        });
    }

    void ChangeOptions()
    {
        if (_currentDisplaySetting.Equals(_beforeChangeDisplaySetting)
            && _currentAudioSetting.Equals(_beforeChangeAudioSetting)
            && _currentMouseSetting.Equals(_beforeChangeMouseSetting))
        {
            _isChangedOptions = false;
            GetTMP((int)Texts.Text_Save).color = Color.gray;
        }
        else
        {
            _isChangedOptions = true;
            GetTMP((int)Texts.Text_Save).color = Color.white;
        }
    }

    void ClickSaveButton(PointerEventData evt)
    {
        if (_isChangedOptions == false)
            return;

        _currentDisplaySetting = _beforeChangeDisplaySetting;
        _currentAudioSetting = _beforeChangeAudioSetting;
        _currentMouseSetting = _beforeChangeMouseSetting;

        Screen.SetResolution(_resolutions[_currentDisplaySetting.resolutionIndex].width, _resolutions[_currentDisplaySetting.resolutionIndex].height, _currentDisplaySetting.screenMode);

        Managers.Camera.CameraSensitivity = _currentMouseSetting.sensitivity;
        Managers.HotKey.CurrentAimingLineIndex = _currentMouseSetting.aimingLineIndex;
        Managers.HotKey.CurrentAimingLineAlpha = _currentMouseSetting.aimingLineAlpha;

        ChangeOptions();
    }
    #endregion

    #region public
    string GetName(UIComponents uINames) => Enum.GetName(typeof(UIComponents), uINames);

    void ExitButton(PointerEventData evt)
    {
        _currentSequence?.Kill();
    }
    #endregion
}
