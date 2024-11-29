using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMapMarks : MonoBehaviour
{
    public SpriteRenderer[] Tiles;
    [Serializable]
    public struct SwitchTileData
    {
        public int from;
        public int to;
        public Transform Transform;
        public Vector3 Size;
        public Vector3 Direction;
    }
    public SwitchTileData[] SwitchTileDatas;

    private void Start()
    {
        Color color = Tiles[1].color;
        color.a = 0;
        Tiles[1].color = color;

    }

    private void FixedUpdate()
    {
        for (int i = 0; i < SwitchTileDatas.Length; i++)
        {
            if (SwitchTileDatas[i].Transform != null)
            {
                Collider[] colliders = Physics.OverlapBox(SwitchTileDatas[i].Transform.position, SwitchTileDatas[i].Size / 2, Quaternion.identity, Managers.Layer.PlayerLayerMask);
                if (colliders.Length > 0)
                {
                    CheckIncludedMapTile(colliders[0].transform.position);
                }
            }
        }
    }

    public void CheckIncludedMapTile(Vector3 position)
    {
        for (int i = 0; i < SwitchTileDatas.Length; i++)
        {
            if (SwitchTileDatas[i].Transform != null)
            {
                if (SwitchTileDatas[i].Transform.position.x + SwitchTileDatas[i].Size.x / 2 >= position.x &&
                    SwitchTileDatas[i].Transform.position.x - SwitchTileDatas[i].Size.x / 2 <= position.x &&
                    SwitchTileDatas[i].Transform.position.y + SwitchTileDatas[i].Size.y / 2 >= position.y &&
                    SwitchTileDatas[i].Transform.position.y - SwitchTileDatas[i].Size.y / 2 <= position.y &&
                    SwitchTileDatas[i].Transform.position.z + SwitchTileDatas[i].Size.z / 2 >= position.z &&
                    SwitchTileDatas[i].Transform.position.z - SwitchTileDatas[i].Size.z / 2 <= position.z)
                {
                    var localPosition = new Vector3((position.x - SwitchTileDatas[i].Transform.position.x) * SwitchTileDatas[i].Direction.x
                        , (position.y - SwitchTileDatas[i].Transform.position.y) * SwitchTileDatas[i].Direction.y
                        , (position.z - SwitchTileDatas[i].Transform.position.z) * SwitchTileDatas[i].Direction.z);
                    var halfSize = new Vector3(SwitchTileDatas[i].Size.x * Mathf.Abs(SwitchTileDatas[i].Direction.x)
                        , SwitchTileDatas[i].Size.y * Mathf.Abs(SwitchTileDatas[i].Direction.y)
                        , SwitchTileDatas[i].Size.z * Mathf.Abs(SwitchTileDatas[i].Direction.z)) / 2;
                    var t = 1 - (localPosition + halfSize).magnitude / (halfSize * 2).magnitude;
                    SwitchMapTile(Tiles[SwitchTileDatas[i].from], Tiles[SwitchTileDatas[i].to], t);
                }
            }
        }
    }

    public void SwitchMapTile(SpriteRenderer tile1, SpriteRenderer tile2, float t)
    {
        Color color = tile1.color;
        color.a = t;
        tile1.color = color;
        color.a = 1 - t;
        tile2.color = color;
    }

    private void OnDrawGizmos()
    {

        for (int i = 0; i < SwitchTileDatas.Length; i++)
        {
            if (SwitchTileDatas[i].Transform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(SwitchTileDatas[i].Transform.position, SwitchTileDatas[i].Size);
                Gizmos.color = Color.red;
                var halfSize = SwitchTileDatas[i].Size / 2;
                Gizmos.DrawRay(SwitchTileDatas[i].Transform.position, -new Vector3(SwitchTileDatas[i].Direction.x * halfSize.x, 0, SwitchTileDatas[i].Direction.z * halfSize.z));
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(SwitchTileDatas[i].Transform.position, new Vector3(SwitchTileDatas[i].Direction.x * halfSize.x, 0, SwitchTileDatas[i].Direction.z * halfSize.z));
            }
        }
    }
}
