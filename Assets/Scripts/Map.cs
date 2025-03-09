using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Map
{
    public Tile[] tiles; // ��ü Ÿ�ϵ��� �����ϴ� �迭
    public int columns;  // ���� ���� ũ��
    public int rows;     // ���� ���� ũ��

    private bool castleSpwn = false; // ���� �����Ǿ����� ����
    public bool playerSpwn = false;  // �÷��̾ �����Ǿ����� ����



    // �ؾȰ� Ÿ��(Ư�� ID ���� ���� �ִ� Ÿ��) ���͸�
    public Tile[] CoastTiles => tiles.Where(x => x.autoTileId >= 0 && x.autoTileId < (int)TileTypes.Grass).ToArray();
    // ���� Ÿ�� ���͸�
    public Tile[] LandTiles => tiles.Where(x => x.autoTileId >= (int)TileTypes.Grass).ToArray();

    // ���ο� ���� �����ϴ� �޼���
    public void NewMap(int width, int height)
    {
        columns = width;  // ���� ���� ũ�� ����
        rows = height;    // ���� ���� ũ�� ����
        tiles = new Tile[rows * columns]; // Ÿ�� �迭 �ʱ�ȭ
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile(); // �� Ÿ�� ��ü ����
            tiles[i].id = i;       // Ÿ�� ID �ο� (1���� �迭�� �ε���)
        }

        // �� Ÿ�Ͽ� ���� �����¿� �̿��� ����
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int index = r * columns + c; // ���� Ÿ���� �迭 �� ��ġ

                if (r - 1 >= 0) // ���� Ÿ�� ����
                {
                    tiles[index].SetNeighbor(Sides.Up, tiles[index - columns]);
                }
                if (c + 1 < columns) // ������ Ÿ�� ����
                {
                    tiles[index].SetNeighbor(Sides.Right, tiles[index + 1]);
                }
                if (r + 1 < rows) // �Ʒ��� Ÿ�� ����
                {
                    tiles[index].SetNeighbor(Sides.Down, tiles[index + columns]);
                }
                if (c - 1 >= 0) // ���� Ÿ�� ����
                {
                    tiles[index].SetNeighbor(Sides.Left, tiles[index - 1]);
                }
            }
        }

        // ��� Ÿ���� �ڵ� Ÿ�� ID ������Ʈ
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].UpdateAutoTileId();
        }
    }

    public void UpdateAutoFowTileId()
    {
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int index = r * columns + c; // ���� Ÿ���� �迭 �� ��ġ
                tiles[index].fowTileId = 0;
                int count = 1 << (int)Sides.Up;
                if (r - 1 >= 0) // ���� Ÿ�� ����
                {
                    // Count �� 1�� 4���� 'LeftShift' �Ͽ� 8�� ���� �� �ٽ� Side�� Up �� 4 ��ŭ 'RightShift'�Ͽ� 1�� �����.
                    tiles[index].fowTileId |= count >> (int)Sides.Up;
                }
                if (c + 1 < columns) // ������ Ÿ�� ����
                {
                    // Count �� 1�� 4���� 'LeftShift' �Ͽ� 8�� ���� �� �ٽ� Side�� Right�� 1��ŭ 'RightShift'�Ͽ� 4�� �����.
                    tiles[index].fowTileId |= count >> (int)Sides.Right;
                }
                if (r + 1 < rows) // �Ʒ��� Ÿ�� ����
                {
                    tiles[index].fowTileId |= count >> (int)Sides.Down; // Count �� 1�� 4���� 'LeftShift' �Ͽ� 8�� ���� �� �ٽ� Side�� Down�� 0��ŭ 'RightShift'�Ͽ� 8�� �����.
                }
                if (c - 1 >= 0) // ���� Ÿ�� ����
                {
                    tiles[index].fowTileId |= count >> (int)Sides.Left; // Count �� 1�� 4���� 'LeftShift' �Ͽ� 8�� ���� �� �ٽ� Side�� Left�� 2��ŭ 'RightShift'�Ͽ� 2�� �����.
                }
            }
        }
    }
    public void ClearsFogPlayerAround(int index, int array)
    {
        int centerRow = index / columns;
        int centerCol = index % columns;

        for (int r = centerRow - array; r <= centerRow + array; r++)
        {
            for (int c = centerCol - array; c <= centerCol + array; c++)
            {
                // ��踦 ����� �ʴ��� Ȯ��
                if (r >= 0 && r < rows && c >= 0 && c < columns)
                {
                    int deleteIndex = r * columns + c;
                    tiles[deleteIndex].foggy = false;
                }
            }
        }
    }
    // ������� �����ϴ� �޼���
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
        // ���� Ÿ�� �� �Ϻθ� ȣ��(�� ����)�� ����
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        // �ؾ� Ÿ���� ħ�Ľ��Ѽ� ���� �ڿ������� ����
        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        // ���� ���� Ÿ���� �����ϰ� ����, ���, ����, ��, ������ ����
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hill);
        DecorateTiles(LandTiles, DungeonPercent, TileTypes.Dungeon);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountain);
        DecorateTiles(LandTiles, townPercent, TileTypes.Town);

        return true;
    }

    // Ư�� ������ Ÿ���� ������ Ÿ�� Ÿ������ �����ϴ� �Լ�
    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        SuffleTiles(tiles); // Ÿ�� �迭�� �������� ����

        int total = Mathf.FloorToInt(tiles.Length * percent); // ������ Ÿ�� ���� ���

        for (int i = 0; i < total; i++)
        {
            TileTypes currentTileType = tileType; // ������ Ÿ�� Ÿ���� ����

            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].ClearNeighbor(); // �ֺ� Ÿ�ϰ��� ���� ����
                    break;
                case TileTypes.Town:
                    if (!castleSpwn) // ù ��° ������ ������ ����
                    {
                        castleSpwn = true;
                        currentTileType = TileTypes.Castle;
                    }
                    else if (!playerSpwn) // �� ��° ������ �÷��̾� ���� ��ġ�� ���
                    {
                        playerSpwn = true;
                    }
                    break;
            }
            tiles[i].autoTileId = (int)currentTileType; // ����� Ÿ�� ID ����
        }
    }

    // �迭�� �������� ���� �Լ� (Fisher-Yates Shuffle �˰���)
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

    // BFS (�ʺ� �켱 Ž��) ��� ã��
    public bool PathFindingBFS(Tile start, Tile end)
    {
        var visited = new HashSet<Tile>(); // �湮�� Ÿ�� ����
        var queue = new Queue<Tile>(); // Ž���� Ÿ�� ť

        queue.Enqueue(start); // ���� Ÿ�� �߰�
        while (queue.Count > 0)
        {
            var currentMap = queue.Dequeue(); // ť���� Ÿ�� ����
            if (currentMap == end) // ��ǥ Ÿ���̸� ��� ����
            {
                return true;
            }

            visited.Add(currentMap); // �湮 ó��
            foreach (var tile in currentMap.neighbors) // �̿� Ÿ�� Ž��
            {
                if (tile == null || visited.Contains(tile))
                    continue;
                queue.Enqueue(tile);
            }
        }
        return false; // ��ΰ� ������ false ��ȯ
    }

    // �޸���ƽ �Լ� (����ư �Ÿ� ���)
    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % columns;
        int ay = a.id / columns;
        int bx = b.id % columns;
        int by = b.id / columns;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by); // �Ÿ� ���
    }

    // A* ��� Ž�� �˰���
    public List<Tile> AStar(Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();

        var visited = new HashSet<Tile>();
        var queue = new PriorityQueue<Tile, int>(Comparer<int>.Create((x, y) => x.CompareTo(y)));

        var distances = new int[tiles.Length];
        var scores = new int[tiles.Length];

        for (int i = 0; i < distances.Length; ++i)
        {
            scores[i] = distances[i] = int.MaxValue;
        }

        distances[start.id] = 0;
        scores[start.id] = Heuristic(start, end);
        queue.Enqueue(start, distances[start.id]);

        bool success = false;
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();
            if (visited.Contains(currentTile)) continue;
            if (currentTile == end)
            {
                success = true;
                break;
            }

            visited.Add(currentTile);
            foreach (var adjacent in currentTile.neighbors)
            {
                if (adjacent == null || adjacent.Weight == int.MaxValue) continue;

                var newDistance = distances[currentTile.id] + adjacent.Weight;
                if (distances[adjacent.id] > newDistance)
                {
                    distances[adjacent.id] = newDistance;
                    scores[adjacent.id] = distances[adjacent.id] + Heuristic(adjacent, end);
                    adjacent.previous = currentTile;
                    queue.Enqueue(adjacent, scores[adjacent.id]);
                }
            }
        }

        if (!success) return path;
        Tile step = end;
        while (step != null)
        {
            path.Add(step);
            step = step.previous;
        }
        path.Reverse();
        return path;
    }
}