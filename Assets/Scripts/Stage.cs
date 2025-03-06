using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Stage : MonoBehaviour
{
    public Sprite[] islandSprites;

    public int mapWidth = 20;
    public int mapHeight = 20;
    public Vector2 tileSize = new Vector2(16, 16);

    public Map map;

    public GameObject tilePrefab;
    public List<GameObject> tileObjs;
    public Vector3 screenPos;
    public GameObject playerPrefab;

    private bool playerSpwn = false;

    private void Start()
    {
        map = new Map();
        MakeMap();
        CreateGrid();
    }
    public int erodeIterations = 3;
    [Range(0, 1)]
    public float erodePercent = 0.2f;
    [Range(0, 1)]
    public float lakePercent = 0.1f;
    [Range(0, 1)]
    public float treePercent = 0.1f;
    [Range(0, 1)]
    public float hillPercent = 0.1f;
    [Range(0, 1)]
    public float TownPercent = 0.1f;
    [Range(0, 1)]
    public float mountainPercent = 0.1f;
    [Range(0, 1)]
    public float DungeonPercent = 0.1f;
    public void MakeMap()
    {
        map.NewMap(mapWidth, mapHeight);
        map.createIsLand(erodeIterations, erodePercent, lakePercent, treePercent, hillPercent, TownPercent, mountainPercent, DungeonPercent);
        Debug.Log("�� ����");
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var tileId = ScreenPosToTileId(Input.mousePosition);

            Debug.Log(tileId);
            Debug.Log(GetTilePos(tileId));
        }
    }
    public int ScreenPosToTileId(Vector3 screenPos) // ��ũ�� ��ǥ���� Ÿ�� ���̵�
    {
        return WorldPosToTileId(Camera.main.ScreenToWorldPoint(screenPos));
    }
    public int WorldPosToTileId(Vector3 worldPos)
    {
        var local = worldPos - transform.position; // ���� ������ ���ϱ�
        int xIndex = (int)(local.x / tileSize.x);
        int yIndex = (int)(-local.y / tileSize.y);
        int x = Mathf.Clamp(xIndex, 0, mapWidth - 1);
        int y = Mathf.Clamp(yIndex, 0, mapHeight - 1);
        return y * mapWidth + x;
    }
    public Vector3 GetTilePos(int y, int x) // 2���� ��ǥ �׸��� ������ �Է¹޾� Ÿ���� ������ǥ ����
    {
        return GetTilePos(y * mapWidth + x);
    }
    public Vector3 GetTilePos(int tileId) // Ÿ�� ���̵� �Է¹޾� Ÿ���� ������ǥ ����
    {
        return tileObjs[tileId].transform.position;
    }

    public void CreateGrid()
    {
        foreach (var go in tileObjs)
        {
            Destroy(go);
        }
        tileObjs.Clear();
        var startPos = transform.position;
        startPos.x += tileSize.x * 0.5f;
        startPos.y -= tileSize.y * 0.5f;

        var pos = startPos;


        for (int i = 0; i < map.rows; i++)
        {
            for (int j = 0; j < map.columns; j++)
            {
                int id = i * map.columns + j;
                var newGo = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                newGo.name = $"Tile{i},{j}";
                pos.x += tileSize.x;

                tileObjs.Add(newGo);
                DecorateTile(id);
            }
            pos.x = startPos.x;
            pos.y -= tileSize.y;
        }
    }
    public void DecorateTile(int tileId)
    {
        var ren = tileObjs[tileId].GetComponent<SpriteRenderer>();
        if (map.tiles[tileId].autoTileId == (int)TileTypes.Empty)
        {
            ren.sprite = null;
        }
        else if (map.tiles[tileId].autoTileId == (int)TileTypes.Town && map.playerSpwn &&!playerSpwn)
        {
            playerSpwn = true;
            var player = Instantiate(playerPrefab, tileObjs[tileId].transform.position, Quaternion.identity);
            ren.sprite = islandSprites[map.tiles[tileId].autoTileId];
        }
        else
        {
            ren.sprite = islandSprites[map.tiles[tileId].autoTileId];
        }
    }
}
