using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerInfoManager
{
    GameObject _player;
    public GameObject Player
    {
        get
        {
            if (_player == null)
                _player = GameObject.FindWithTag("Player");
            return _player;
        }
    }
    PlayerController _controller;
    public PlayerController Controller
    {
        get
        {
            if (_controller == null)
                _controller = Player?.GetComponent<PlayerController>();
            return _controller;
        }
    }
    public SkillInfo Skill { get { return _skill; } }
    public bool IsDead => Controller.IsDead;
    SkillInfo _skill = new SkillInfo();
    Transform _respawnPoint;

    public void Init()
    {
        _skill.Init();
    }

    public Transform GetRespawnPoint() => _respawnPoint;

    public void SetRespawnPoint(Transform point)
    {
        _respawnPoint = point;
    }

    public void DiePlayer()
    {
        Managers.InfoUI.ShowGameOverWindow();
    }

    public void RespawnPlayer()
    {
        Controller.Respawn(_respawnPoint);
    }
}

public partial class PlayerInfoManager
{
    public class SkillInfo
    {
        public void Init()
        {
            InitSkillLevel();
        }

        public Dictionary<Define.SkillName, int> Level;
        int _totalSkillPoint;
        int _currentSkillPoint;
        public int TotalSkillPoint
        {
            get { return _totalSkillPoint; }
            set
            {
                _totalSkillPoint = value;
                _currentSkillPoint += value;
                UpdateSkillPoint?.Invoke();
            }
        }
        public int CurrentSkillPoint
        {
            get { return _currentSkillPoint; }
            set
            {
                _currentSkillPoint = value;
                UpdateSkillPoint?.Invoke();
            }
        }
        public Action UpdateSkillPoint;
        public Action<Define.SkillName, int> UpdateSkillLevel;

        void InitSkillLevel()
        {
            TotalSkillPoint = 10;

            Level = new Dictionary<Define.SkillName, int>();

            int skillCount = Managers.Data.SkillDic.Count - 1;
            for (int i = 1; i <= skillCount; i++)
            {
                Level.Add((Define.SkillName)i, 0);
            }
        }

        public int GetSkillDamage(Define.SkillName skillName)
        {
            return Mathf.RoundToInt((float)Managers.PlayerInfo.Controller.Stat.Damage
                * (Managers.Data.SkillDic[(int)skillName].damage
                + Managers.Data.SkillDic[(int)skillName].increase * Level[skillName]));
        }

        public void UpSkillLevel(Define.SkillName skillName)
        {
            if (Level[skillName] >= 10 || CurrentSkillPoint <= 0)
                return;

            Level[skillName]++;
            CurrentSkillPoint--;
            UpdateSkillLevel?.Invoke(skillName, Level[skillName]);
        }

        public void DownSkillLevel(Define.SkillName skillName)
        {
            if (Level[skillName] <= 0)
                return;
            if (Level[skillName] - 1 == 0)
                if (Managers.HotKey.DeleteSkill(skillName) == false)
                    return;
            Level[skillName]--;
            CurrentSkillPoint++;
            UpdateSkillLevel?.Invoke(skillName, Level[skillName]);
        }
    }
}