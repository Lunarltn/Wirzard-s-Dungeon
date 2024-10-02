using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class LayerManager
{
    public const string GORUND = "Ground";
    public const string OBJECT = "Object";
    public const string SKILL = "Skill";
    public const string PLAYER = "Player";
    public const string MONSTER = "Monster";
    public const string NPC = "NPC";
    public const string INTERECTION = "InteractionObject";
    public const string DETECTION = "DetectionObject";
    public const string MINIMAP = "MiniMap";

    public int GroundLayer { get; private set; }
    public int ObjectLayer { get; private set; }
    public int SkillLayer { get; private set; }
    public int PlayerLayer { get; private set; }
    public int MonsterLayer { get; private set; }
    public int NPCLayer { get; private set; }
    public int InterectionLayer { get; private set; }
    public int DetectionLayer { get; private set; }
    public int MiniMapLayer { get; private set; }

    public int GroundLayerMask => GetMask(GroundLayer);
    public int ObjectLayerMask => GetMask(ObjectLayer);
    public int SkillLayerMask => GetMask(SkillLayer);
    public int PlayerLayerMask => GetMask(PlayerLayer);
    public int MonsterLayerMask => GetMask(MonsterLayer);
    public int NPCLayerMask => GetMask(NPCLayer);
    public int InterectionLayerMask => GetMask(InterectionLayer);
    public int DetectionLayerMask => GetMask(DetectionLayer);
    public int MiniMapLayerMask => GetMask(MiniMapLayer);

    public void Init()
    {
        GroundLayer = LayerMask.NameToLayer(GORUND);
        ObjectLayer = LayerMask.NameToLayer(OBJECT);
        SkillLayer = LayerMask.NameToLayer(SKILL);
        PlayerLayer = LayerMask.NameToLayer(PLAYER);
        MonsterLayer = LayerMask.NameToLayer(MONSTER);
        NPCLayer = LayerMask.NameToLayer(NPC);
        InterectionLayer = LayerMask.NameToLayer(INTERECTION);
        DetectionLayer = LayerMask.NameToLayer(DETECTION);
        MiniMapLayer = LayerMask.NameToLayer(MINIMAP);
    }

    int GetMask(int layer) => (1 << layer);
}
