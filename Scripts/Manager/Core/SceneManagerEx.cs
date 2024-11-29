using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManagerEx
{
    public BaseScene CurrentScene { get { return GameObject.FindObjectOfType<BaseScene>(); } }
    public string NextScene;

    public void LoadScene(Define.Scene type)
    {
        if (NextScene == GetSceneName(type)) return;

        NextScene = GetSceneName(type);

        if (CurrentScene.SceneType == Define.Scene.Logo)
            SceneManager.LoadScene(GetSceneName(type));
        else
            FadeScene().Forget();
    }

    async UniTask FadeScene()
    {
        var fade = Managers.UI.ShowSceneUI<UI_FadeEffect>();
        fade.FadeEffect(UI_FadeEffect.Options.FadeOut, 3f).Forget();
        await UniTask.WaitWhile(() => { return !fade.IsStop; });
        LoadingScene();
    }

    void LoadingScene()
    {
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(Define.Scene.Loading));
    }

    public string GetSceneName(Define.Scene type)
    {
        string name = Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
