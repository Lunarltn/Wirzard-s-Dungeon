using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseRoom : MonoBehaviour
{
    [SerializeField]
    protected Vector3 roomPosition;
    [SerializeField]
    protected Vector2 roomSize;
    public Vector3 RoomSizeV3 => new Vector3(roomSize.x, 0, roomSize.y);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(roomPosition, new Vector3(roomSize.x, 0, roomSize.y));
    }
}
