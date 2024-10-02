public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Game;
        Managers.UI.ShowSceneUI<UI_PlayerStat>();
        Managers.UI.ShowSceneUI<UI_HotKeySlotBar>();
        //Managers.HotKey.SetSkill(0, Managers.Data.SkillDic[(int)Define.SkillName.Fireball]);
    }

    private void Update()
    {

    }

    public override void Clear()
    {

    }
}
