using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;



public class Map : MonoBehaviour
{
    public Tile[] tiles;
    public int columns;
    public int rows;

    private bool castleSpwn = false;
    public bool playerSpwn = false;

    public Tile[] CoastTiles => tiles.Where(x => x.autoTileId >= 0 && x.autoTileId < (int)TileTypes.Grass).ToArray();
    public Tile[] LandTiles => tiles.Where(x => x.autoTileId >= (int)TileTypes.Grass).ToArray();
    public void NewMap(int width, int height)
    {
        columns = width;
        rows = height;

        tiles = new Tile[rows * columns];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile();
            tiles[i].id = i;
        }
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int index = r * columns + c;

                if (r - 1 >= 0)
                {
                    tiles[index].SetNeighbor(Sides.Up, tiles[index - columns]);
                }
                if (c + 1 < columns)
                {
                    tiles[index].SetNeighbor(Sides.Right, tiles[index + 1]);

                }
                if (r + 1 < rows)
                {
                    tiles[index].SetNeighbor(Sides.Down, tiles[index + columns]);

                }
                if (c - 1 >= 0)
                {
                    tiles[index].SetNeighbor(Sides.Left, tiles[index - 1]);

                }
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAutoTileId();
        }
    }

    public bool createIsLand(
        int erodeIterations,
        float erodePercent,
        float lakePercent,
        float treePercent,
        float hillPercent,
        float townPercent,
        float mountainPercent,
        float DungeonPercent)
    {
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hill);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountain);
        DecorateTiles(LandTiles, townPercent, TileTypes.Town);
        DecorateTiles(LandTiles, DungeonPercent, TileTypes.Dungeon);

        return true;
    }
    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        SuffleTiles(tiles);

        int total = Mathf.FloorToInt(tiles.Length * percent);

        for (int i = 0; i < total; i++)
        {
            TileTypes currentTileType = tileType; // 별도 변수 사용

            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].ClearNeighbor();
                    break;
                case TileTypes.Town:
                    if (!castleSpwn)
                    {
                        castleSpwn = true;
                        currentTileType = TileTypes.Castle; // 여기서만 변경
                    }
                    else if (!playerSpwn)
                    {
                        playerSpwn = true;
                    }
                    break;
            }
            tiles[i].autoTileId = (int)currentTileType; // 원본 tileType을 유지
        }
    }

    public void SuffleTiles(Tile[] tiles)
    {
        for (int i = tiles.Length - 1; i >= 0; i--)
        {
            int ren = Random.Range(0, i + 1);

            Tile tile = tiles[i];
            tiles[i] = tiles[ren];
            tiles[ren] = tile;
        }
    }

    public bool PathFindingBFS(Tile start, Tile end)
    {
        var visited = new HashSet<Tile>();
        var queue = new Queue<Tile>();

        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            var currentMap = queue.Dequeue();
            if (currentMap == end)
            {
                return true;
            }

            visited.Add(currentMap);
            foreach (var tile in currentMap.neighbors)
            {
                if (tile == null||visited.Contains(tile))
                    continue;
                queue.Enqueue(tile);
            }
        }
        return false;
    }
}

