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
    Up,
}

public class Tile
{
    public int id = 0;
    public Tile[] neighbors = new Tile[4];

    public int autoTileId = 0;
    public void SetNeighbor(Sides sides, Tile neighbor)
    {
        neighbors[(int)sides] = neighbor;
    }

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
