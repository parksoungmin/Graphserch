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
    public Tile[] tiles; // 전체 타일들을 저장하는 배열
    public int columns;  // 맵의 가로 크기
    public int rows;     // 맵의 세로 크기

    private bool castleSpwn = false; // 성이 생성되었는지 여부
    public bool playerSpwn = false;  // 플레이어가 스폰되었는지 여부



    // 해안가 타일(특정 ID 범위 내에 있는 타일) 필터링
    public Tile[] CoastTiles => tiles.Where(x => x.autoTileId >= 0 && x.autoTileId < (int)TileTypes.Grass).ToArray();
    // 육지 타일 필터링
    public Tile[] LandTiles => tiles.Where(x => x.autoTileId >= (int)TileTypes.Grass).ToArray();

    // 새로운 맵을 생성하는 메서드
    public void NewMap(int width, int height)
    {
        columns = width;  // 맵의 가로 크기 설정
        rows = height;    // 맵의 세로 크기 설정
        tiles = new Tile[rows * columns]; // 타일 배열 초기화
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = new Tile(); // 각 타일 객체 생성
            tiles[i].id = i;       // 타일 ID 부여 (1차원 배열의 인덱스)
        }

        // 각 타일에 대해 상하좌우 이웃을 설정
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                int index = r * columns + c; // 현재 타일의 배열 내 위치

                if (r - 1 >= 0) // 위쪽 타일 연결
                {
                    tiles[index].SetNeighbor(Sides.Up, tiles[index - columns]);
                }
                if (c + 1 < columns) // 오른쪽 타일 연결
                {
                    tiles[index].SetNeighbor(Sides.Right, tiles[index + 1]);
                }
                if (r + 1 < rows) // 아래쪽 타일 연결
                {
                    tiles[index].SetNeighbor(Sides.Down, tiles[index + columns]);
                }
                if (c - 1 >= 0) // 왼쪽 타일 연결
                {
                    tiles[index].SetNeighbor(Sides.Left, tiles[index - 1]);
                }
            }
        }

        // 모든 타일의 자동 타일 ID 업데이트
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
                int index = r * columns + c; // 현재 타일의 배열 내 위치
                tiles[index].fowTileId = 0;
                int count = 1 << (int)Sides.Up;
                if (r - 1 >= 0) // 위쪽 타일 연결
                {
                    // Count 는 1을 4번을 'LeftShift' 하여 8로 만든 후 다시 Side의 Up 인 4 만큼 'RightShift'하여 1로 만든다.
                    tiles[index].fowTileId |= count >> (int)Sides.Up;
                }
                if (c + 1 < columns) // 오른쪽 타일 연결
                {
                    // Count 는 1을 4번을 'LeftShift' 하여 8로 만든 후 다시 Side의 Right인 1만큼 'RightShift'하여 4로 만든다.
                    tiles[index].fowTileId |= count >> (int)Sides.Right;
                }
                if (r + 1 < rows) // 아래쪽 타일 연결
                {
                    tiles[index].fowTileId |= count >> (int)Sides.Down; // Count 는 1을 4번을 'LeftShift' 하여 8로 만든 후 다시 Side의 Down인 0만큼 'RightShift'하여 8로 만든다.
                }
                if (c - 1 >= 0) // 왼쪽 타일 연결
                {
                    tiles[index].fowTileId |= count >> (int)Sides.Left; // Count 는 1을 4번을 'LeftShift' 하여 8로 만든 후 다시 Side의 Left인 2만큼 'RightShift'하여 2로 만든다.
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
                // 경계를 벗어나지 않는지 확인
                if (r >= 0 && r < rows && c >= 0 && c < columns)
                {
                    int deleteIndex = r * columns + c;
                    tiles[deleteIndex].foggy = false;
                }
            }
        }
    }
    // 랜드맵을 생성하는 메서드
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
        // 육지 타일 중 일부를 호수(빈 공간)로 변경
        DecorateTiles(LandTiles, lakePercent, TileTypes.Empty);

        // 해안 타일을 침식시켜서 맵을 자연스럽게 만듦
        for (int i = 0; i < erodeIterations; i++)
        {
            DecorateTiles(CoastTiles, erodePercent, TileTypes.Empty);
        }

        // 남은 육지 타일을 랜덤하게 나무, 언덕, 던전, 산, 마을로 변경
        DecorateTiles(LandTiles, treePercent, TileTypes.Tree);
        DecorateTiles(LandTiles, hillPercent, TileTypes.Hill);
        DecorateTiles(LandTiles, DungeonPercent, TileTypes.Dungeon);
        DecorateTiles(LandTiles, mountainPercent, TileTypes.Mountain);
        DecorateTiles(LandTiles, townPercent, TileTypes.Town);

        return true;
    }

    // 특정 비율의 타일을 지정한 타일 타입으로 변경하는 함수
    public void DecorateTiles(Tile[] tiles, float percent, TileTypes tileType)
    {
        SuffleTiles(tiles); // 타일 배열을 랜덤으로 섞음

        int total = Mathf.FloorToInt(tiles.Length * percent); // 변경할 타일 개수 계산

        for (int i = 0; i < total; i++)
        {
            TileTypes currentTileType = tileType; // 변경할 타일 타입을 저장

            switch (tileType)
            {
                case TileTypes.Empty:
                    tiles[i].ClearNeighbor(); // 주변 타일과의 연결 해제
                    break;
                case TileTypes.Town:
                    if (!castleSpwn) // 첫 번째 마을은 성으로 변경
                    {
                        castleSpwn = true;
                        currentTileType = TileTypes.Castle;
                    }
                    else if (!playerSpwn) // 두 번째 마을은 플레이어 스폰 위치로 사용
                    {
                        playerSpwn = true;
                    }
                    break;
            }
            tiles[i].autoTileId = (int)currentTileType; // 변경된 타일 ID 적용
        }
    }

    // 배열을 랜덤으로 섞는 함수 (Fisher-Yates Shuffle 알고리즘)
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

    // BFS (너비 우선 탐색) 경로 찾기
    public bool PathFindingBFS(Tile start, Tile end)
    {
        var visited = new HashSet<Tile>(); // 방문한 타일 저장
        var queue = new Queue<Tile>(); // 탐색할 타일 큐

        queue.Enqueue(start); // 시작 타일 추가
        while (queue.Count > 0)
        {
            var currentMap = queue.Dequeue(); // 큐에서 타일 꺼냄
            if (currentMap == end) // 목표 타일이면 경로 존재
            {
                return true;
            }

            visited.Add(currentMap); // 방문 처리
            foreach (var tile in currentMap.neighbors) // 이웃 타일 탐색
            {
                if (tile == null || visited.Contains(tile))
                    continue;
                queue.Enqueue(tile);
            }
        }
        return false; // 경로가 없으면 false 반환
    }

    // 휴리스틱 함수 (맨해튼 거리 사용)
    private int Heuristic(Tile a, Tile b)
    {
        int ax = a.id % columns;
        int ay = a.id / columns;
        int bx = b.id % columns;
        int by = b.id / columns;

        return Mathf.Abs(ax - bx) + Mathf.Abs(ay - by); // 거리 계산
    }

    // A* 경로 탐색 알고리즘
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