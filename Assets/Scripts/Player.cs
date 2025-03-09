using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public Animator animator;
    public float speed = 2f;
    public Map map;
    public Stage stage;

    public void Update()
    {
        //if (Input.GetMouseButtonDown(0))
        //{
        //    var tileId = stage.ScreenPosToTileId(Input.mousePosition);

        //    Debug.Log(tileId);
        //    Debug.Log(stage.GetTilePos(tileId));
        //    Debug.Log(map.tiles[tileId].autoTileId);
        //}
    }
}
