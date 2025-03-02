using System.Collections.Generic;
using UnityEngine;

public class GameMap
{
    GameObject grid = null;
    GameObject bg = null;
    UIRoot uiRoot = null;

    List<GameObject> walls;

    public UIRoot UiRoot { get => uiRoot;}
    public GameObject Bg { get => bg;}

    public void OnInitLayout(GameObject grid)
    {
        this.grid = grid;
        walls = new List<GameObject>();
        CreateUIRoot(ResManager.Instance.uiRootPrefab);
        CreateBG(ResManager.Instance.slotBGPrefab);
        CreteSlotBG(ResManager.Instance.slotPrefab,grid);
        CreateWall(ResManager.Instance.wall);
        CreateButtomWall(ResManager.Instance.buttomWall);
    }

    void CreateUIRoot(UIRoot uiRootPrefab)
    {
        uiRoot = GameObject.Instantiate(uiRootPrefab);
    }

    void CreateBG(GameObject slotBGPrefab)
    {
        bg = GameObject.Instantiate(slotBGPrefab);
    }

    void CreteSlotBG(GameObject slotPrefab, GameObject grid)
    {
        Vector3 startPos = new Vector3(-1f, 0.03f, 0);
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.col; j++)
            {
                GameObject slot = GameObject.Instantiate(slotPrefab);
                slot.transform.SetParent(grid.transform);
                slot.transform.position = startPos + new Vector3(.68f * i, -.64f * j, 0);
            }
        }
    }

    //生成底下的墙
    void CreateButtomWall(GameObject buttomWall)
    {
        Vector3 pos = GameCfg.buttomWallStartPos;
        //从左到右依次生成底部的墙
        for (int i = 0; i < GameCfg.buttomWallNum; i++)
        {
            GameObject.Instantiate(buttomWall,pos,Quaternion.identity,bg.transform);
            pos += new Vector3(.25f, 0, 0);
        }
    }

    //生成两边的墙
    void CreateWall(GameObject wall)
    {
        Vector3 pos = GameCfg.wall[0];
        //生成左边的墙
        for (int i = 0; i < 15; i++)
        {
            GameObject.Instantiate(wall, pos, Quaternion.identity, bg.transform);
            pos += new Vector3(0,.28f, 0);
        }
        pos = GameCfg.wall[1];
        //生成右边的墙
        for (int i = 0; i < 15; i++)
        {
            walls.Add(GameObject.Instantiate(wall, pos, Quaternion.identity, bg.transform));
            pos += new Vector3(0, .28f, 0);
        }
    }

    public bool DestroyWall()
    {
        walls[GameCfg.wallNum-1].SetActive(false);
        GameCfg.wallNum--;   
        return GameCfg.wallNum <= 0;
    }

    /// <summary>
    /// 得到当前砖块的位置
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurrentWallPos()
    {
        return this.grid.transform.TransformPoint(walls[GameCfg.wallNum - 1].transform.position);
    }

    /// <summary>
    /// 重新布局地图
    /// </summary>
    public void OnRecreate()
    {
        //TODO:
    }

    public void OnDestroy()
    {
        grid = null;
        bg = null;
        uiRoot = null;
    }
}
