using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnRoom : BaseRoom
{
    const string PATH = "Monster/";

    [SerializeField]
    int spawnCount;
    [SerializeField]
    float spawnTime;
    [SerializeField]
    string monsterName;


    int _currentSpawnCount;
    float _spawnTimer;
    List<BaseController> _monstersController;

    private void Start()
    {
        _monstersController = new List<BaseController>();
        var monsters = Physics.OverlapBox(roomPosition, (RoomSizeV3 / 2) + Vector3.up, Quaternion.identity, Managers.Layer.MonsterLayerMask);
        for (int i = 0; i < monsters.Length; i++)
            _monstersController.Add(monsters[i].attachedRigidbody.GetComponent<BaseController>());
        _currentSpawnCount = monsters.Length;
        _spawnTimer = 0;
    }

    private void Update()
    {
        _spawnTimer += Time.deltaTime;

        if (_spawnTimer > spawnTime)
        {
            _spawnTimer = 0;

            for (int i = 0; i < _monstersController.Count; i++)
            {
                if (_monstersController[i].IsDead)
                    _monstersController.RemoveAt(i);
            }
            _currentSpawnCount = _monstersController.Count;
            if (_currentSpawnCount < spawnCount)
                SpawnRandomPosition();
        }
    }
    void SpawnRandomPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randPos = roomPosition + new Vector3(
                Random.Range(roomSize.x * -0.5f, roomSize.x * 0.5f),
                0,
                Random.Range(roomSize.y * -0.5f, roomSize.y * 0.5f));
            if (NavMesh.SamplePosition(randPos, out NavMeshHit hit, 100, NavMesh.AllAreas))
            {
                var monster = Managers.Resource.Instantiate(PATH + monsterName);
                var controller = monster.GetComponent<MonsterController>();
                controller.InitSpawnSetting(hit.position);
                _monstersController.Add(controller);
                break;
            }
        }
    }
}
