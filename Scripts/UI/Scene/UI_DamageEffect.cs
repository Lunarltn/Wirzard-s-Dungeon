using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;

public class UI_DamageEffect : UI_Scene
{
    const string MISS = "Miss!";
    enum GameObjects
    {
        _TextGroup
    }

    TextMeshProUGUI[] _textEffects;

    public override bool Init()
    {
        BindObject(typeof(GameObjects));

        var textGroupTransform = GetObject(0).transform;
        _textEffects = new TextMeshProUGUI[textGroupTransform.childCount];
        for (int i = 0; i < _textEffects.Length; i++)
        {
            _textEffects[i] = textGroupTransform.GetChild(i).GetComponent<TextMeshProUGUI>();
            _textEffects[i].gameObject.SetActive(false);
        }

        return true;
    }

    public void ShowDamageText(Vector3 position, Damage damage)
    {
        if (damage.IsIgnored)
            return;
        var dir = (position - Managers.Camera.MainCamera.transform.position).normalized;
        var dis = Vector3.Distance(position, Managers.Camera.MainCamera.transform.position);
        Ray ray = new Ray(Managers.Camera.MainCamera.transform.position, dir);
        if (Physics.Raycast(ray, dis, Managers.Layer.GroundLayerMask))
            return;

        int idx = -1;
        for (int i = 0; i < _textEffects.Length; i++)
        {
            if (_textEffects[i].gameObject.activeSelf == false)
            {
                _textEffects[i].gameObject.SetActive(true);
                idx = i;
                break;
            }
        }
        if (idx == -1)
            return;

        if (damage.IsCritical && damage.Value > 0)
        {
            _textEffects[idx].fontSize = 36;
            _textEffects[idx].color = Color.red;
        }
        else
        {
            _textEffects[idx].fontSize = 40;
            _textEffects[idx].color = Color.white;
        }

        if (damage.Value == 0)
            _textEffects[idx].text = MISS;
        else
            _textEffects[idx].text = damage.Value.ToString();

        var screenPos = Managers.Camera.MainCamera.WorldToScreenPoint(position);
        _textEffects[idx].transform.position = screenPos;

        AnimationDamage(screenPos, idx);
    }

    void AnimationDamage(Vector3 position, int idx)
    {
        DOTween.Sequence()
        .AppendInterval(0.1f)
       .OnStart(() =>
       {
           _textEffects[idx].alpha = 1;
       })
       .Append(_textEffects[idx].transform.DOMove(position + Vector3.up * 100f, 1f))
       .Insert(0.2f, _textEffects[idx].DOFade(0, 0.8f))
       .OnComplete(() =>
       {
           _textEffects[idx].gameObject.SetActive(false);
       });
    }
}
