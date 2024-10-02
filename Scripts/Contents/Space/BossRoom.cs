using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BossRoom : BaseRoom
{
    [SerializeField]
    List<Door> _door;

    public Vector3 RoomPosition => roomPosition;
    public Vector2 RoomSize => roomSize;
    private void Update()
    {
        foreach (Door door in _door)
        {
            if (door == null) return;

            if (door.IsOpen && IsInsideRoom(Managers.PlayerInfo.Player.transform.position))
            {
                door.CloseDoor();
                door.Lock();
                Debug.Log("¾È");
            }
            if (door.UsedKey && GetDistanceFromRoom(Managers.PlayerInfo.Player.transform.position) > 20f)
            {
                door.UnLock();
                Debug.Log("¹Û");
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

    public bool IsInsideRoom(Vector3 position)
    {
        return (roomPosition.x + roomSize.x / 2 > position.x && roomPosition.x - roomSize.x / 2 < position.x
            && roomPosition.z + roomSize.y / 2 > position.z && roomPosition.z - roomSize.y / 2 < position.z);
    }

    public float GetDistanceFromRoom(Vector3 position)
    {
        float distance = 0f;

        if (IsInsideRoom(position) == false)
        {
            Vector3 dir = (position - transform.position).normalized;
            Vector3 roomOutlinePos = new Vector3(roomPosition.x + dir.x * (roomSize.x / 2), 0, roomPosition.z + dir.z * (roomSize.y / 2));
            distance = Vector3.Distance(position, roomOutlinePos);
        }

        return distance;
    }
}
