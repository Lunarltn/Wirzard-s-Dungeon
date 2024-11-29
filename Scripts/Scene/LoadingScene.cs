using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : BaseScene
{
    [SerializeField]
    UI_LoadingBar loadingBar;

    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Loading;
        LoadScene().Forget();
    }

    async UniTask LoadScene()
    {
        if (Managers.Scene.NextScene == null) return;
        await UniTask.Yield();
        AsyncOperation op = SceneManager.LoadSceneAsync(Managers.Scene.NextScene);
        op.allowSceneActivation = false;
        float timer = 0;
        while (!op.isDone)
        {
            await UniTask.Yield();
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                loadingBar.FillAmount(Mathf.Lerp(loadingBar.fillAmount, op.progress, timer));
                if (loadingBar.fillAmount >= op.progress)
                    timer = 0;
            }
            else
            {
                loadingBar.FillAmount(Mathf.Lerp(loadingBar.fillAmount, 1, timer));
                if (loadingBar.fillAmount == 1.0f)
                {
                    op.allowSceneActivation = true;
                    break;
                }
            }
        }
    }

    public override void Clear()
    {
    }
}
