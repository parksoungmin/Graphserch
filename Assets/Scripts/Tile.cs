using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum TileTypes
{
    Empty = -1,
    Grass = 15,
    Tree,
    Hill,
    Mountain,
    Town,
    Castle,
    Dungeon,
}

public enum Sides
{
    Down,
    Right,
    Left,
    Up
}

public class Tile
{
    public static readonly int[] tableWeight =
    {
        1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
        2, 4, int.MaxValue, 1, 1, 1
    };
    public int Weight
    {
        get
        {
            if (autoTileId == -1)
            {
                return int.MaxValue;
            }
            return tableWeight[autoTileId];
        }
    }
    public int id = 0;
    public Tile[] neighbors = new Tile[4]; // �� ���� �̿�
    public Tile previous = null;  // ��� Ž�� �� ���

    public int autoTileId = 0; // Ÿ�� ���� ID
    public int fowTileId = 15;

    public bool foggy = true;

    // �̿� Ÿ�� ����
    public void SetNeighbor(Sides sides, Tile neighbor)
    {
        neighbors[(int)sides] = neighbor;
    }
    // �̿� ������ �������� �ڵ� Ÿ�� ID ������Ʈ
    public void UpdateAutoTileId()
    {
        autoTileId = 0;
        for (int i = 0; i < neighbors.Length; i++)
        {
            autoTileId = autoTileId << 1;
            if (neighbors[i] != null)
            {
                ++autoTileId;
            }
        }
    }
    public void RemoveNeighbor(Tile tile)
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] == tile)
            {
                neighbors[i] = null;
            }
        }
        UpdateAutoTileId();
    }

    public void ClearNeighbor()
    {
        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] != null)
            {
                neighbors[i].RemoveNeighbor(this);
                neighbors[i] = null;
            }
        }
        UpdateAutoTileId();
    }
}
