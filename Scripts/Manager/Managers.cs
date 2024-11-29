using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
//싱글톤 관리
public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    LayerManager _layer = new LayerManager();
    CameraManager _camera = new CameraManager();
    InformationUIManager _infoUI = new InformationUIManager();
    InputManager _input = new InputManager();
    InventoryManager _inventory = new InventoryManager();
    PlayerInfoManager _playerInfo = new PlayerInfoManager();
    HotKeyManager _hotKey = new HotKeyManager();
    QuestManager _quest = new QuestManager();
    NPCManager _npc = new NPCManager();

    DataManager _data = new DataManager();
    PoolManager _pool = new PoolManager();
    ResourceManager _resource = new ResourceManager();
    SaveManger _save = new SaveManger();
    SceneManagerEx _scene = new SceneManagerEx();
    SoundManager _sound = new SoundManager();
    UIManager _uI = new UIManager();

    public static LayerManager Layer { get { return Instance._layer; } }
    public static CameraManager Camera { get { return Instance._camera; } }
    public static InformationUIManager InfoUI { get { return Instance._infoUI; } }
    public static InputManager Input { get { return Instance._input; } }
    public static InventoryManager Inventory { get { return Instance._inventory; } }
    public static PlayerInfoManager PlayerInfo { get { return Instance._playerInfo; } }
    public static HotKeyManager HotKey { get { return Instance._hotKey; } }
    public static QuestManager Quest { get { return Instance._quest; } }
    public static NPCManager NPC { get { return Instance._npc; } }

    public static DataManager Data { get { return Instance._data; } }
    public static PoolManager Pool { get { return Instance._pool; } }
    public static ResourceManager Resource { get { return Instance._resource; } }
    public static SaveManger Save { get { return Instance._save; } }
    public static SceneManagerEx Scene { get { return Instance._scene; } }
    public static SoundManager Sound { get { return Instance._sound; } }
    public static UIManager UI { get { return Instance._uI; } }

    TestManager _test = new TestManager();
    public static TestManager Test { get { return Instance._test; } }

    void Start()
    {
        Init();
    }

    static void Init()
    {
        if (s_instance == null)
        {
            GameObject go = new GameObject { name = "@Managers" };
            go.AddComponent<Managers>();
            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();

            //core
            s_instance._data.Init();
            s_instance._pool.Init();
            //content
            s_instance._layer.Init();
            s_instance._input.Init();
            SceneManager.sceneLoaded += OnLevelLoaded;
        }
    }

    static void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {

        if (scene.name != Scene.GetSceneName(Define.Scene.Loading))
        {
            var fade = UI.ShowSceneUI<UI_FadeEffect>();
            fade.FadeEffect(UI_FadeEffect.Options.FadeIn, 3f).Forget();
        }

        if (scene.name == Scene.GetSceneName(Define.Scene.Game))
        {
            s_instance._playerInfo.Init();
            s_instance._camera.Init();
            s_instance._inventory.Init();
            s_instance._hotKey.Init();
            s_instance._npc.Init();
            s_instance._quest.Init();
            s_instance._infoUI.Init();
        }
    }

    void Update()
    {
        if (Scene.CurrentScene.SceneType == Define.Scene.Game)
        {
            Input.HandleInput();
            Camera.Update();
        }
        _test.Update();
    }

    public static void Clear()
    {
        Pool.Clear();
        Sound.Clear();
        Scene.Clear();
        NPC.Clear();
        HotKey.Clear();
        UI.Clear();
        Camera.Clear();
        Input.Clear();
    }
}
