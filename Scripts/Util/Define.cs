public class Define
{
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount
    }
    public enum Cursor
    {
        Basic,
        Select
    }
    public enum Scene
    {
        Unknown,
        Lobby,
        Game
    }
    public enum UI_Event
    {
        Down,
        Up,
        Drag,
        Click,
        EndDrag,
        Enter,
        Exit,
        Scroll
    }

    public enum SkillType
    {
        None,
        SingleFront,
        ContinuousFront,
        SingleCasting,
        ContinuousCasting
    }

    public enum SkillName
    {
        Dash,
        Fireball,
        Meteor,
        Firebreath,
        Firewall,
        FlameInjection,
        MagicArrow,
        LightningStrike,
        Shelling
    }

    public enum SkillTarget
    {
        Player,
        Monster
    }

    public enum MainCategory
    {
        None,
        Equip,
        Use,
        Etc,
    }

    public enum EquipCategory
    {
        None,
        Helmet,
        Top,
        Bottom,
        Weapon,
        Gloves,
        Shoes
    }

    public enum QuestCategory
    {
        None,
        Hunt,
        Collect
    }

    public enum QuestStatus
    {
        None,
        Can_Start,
        In_Progress,
        Can_Finish,
        Finished
    }

}