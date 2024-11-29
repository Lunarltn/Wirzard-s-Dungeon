using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BossRoom : BaseRoom
{
    [SerializeField]
    List<Door> _door;
    [SerializeField]
    string monsterName;

    public Vector3 RoomPosition => roomPosition;
    public Vector2 RoomSize => roomSize;

    public bool IsClear;

    private void Update()
    {
        foreach (Door door in _door)
        {
            if (door == null) return;

            if (IsClear == false && door.IsOpen && IsInsideRoom(Managers.PlayerInfo.Player.transform.position, -2 * Vector2.one))
            {
                door.CloseDoor();
                door.Lock();
            }
            if (IsClear || door.UsedKey && GetDistanceFromRoom(Managers.PlayerInfo.Player.transform.position) > 20f)
            {
                door.UnLock();
            }
        }
    }

    public Vector3 GetOppositeDirectionVector(Vector3 position)
    {
        Vector3 currentPosition = position - transform.position;
        if (currentPosition.x > 0 && currentPosition.z > 0)
            return new Vector3(-1, 0, -1);
        else if (currentPosition.x < 0 && currentPosition.z > 0)
            return new Vector3(1, 0, -1);
        if (currentPosition.x > 0 && currentPosition.z < 0)
            return new Vector3(-1, 0, 1);
        else
            return new Vector3(1, 0, 1);
    }

    public bool IsInsideRoom(Vector3 position, Vector2 addSize)
    {
        return (roomPosition.x + (roomSize.x + addSize.x) / 2 > position.x && roomPosition.x - (roomSize.x + addSize.x) / 2 < position.x
            && roomPosition.z + (roomSize.y + addSize.y) / 2 > position.z && roomPosition.z - (roomSize.y + addSize.y) / 2 < position.z);
    }

    public float GetDistanceFromRoom(Vector3 position)
    {
        float distance = 0f;

        if (IsInsideRoom(position, Vector2.zero) == false)
        {
            Vector3 dir = (position - transform.position).normalized;
            Vector3 roomOutlinePos = new Vector3(roomPosition.x + dir.x * (roomSize.x / 2), 0, roomPosition.z + dir.z * (roomSize.y / 2));
            distance = Vector3.Distance(position, roomOutlinePos);
        }

        return distance;
    }
}
