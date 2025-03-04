using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class GameMap
{
    GameObject grid = null;
    GameObject bg = null;
    UIRoot uiRoot = null;

    List<GameObject> walls;
    List<GameObject> leftWalls;
    List<GameObject> bottoWalls;

    List<GameObject> slots;

    public UIRoot UiRoot { get => uiRoot;}
    public GameObject Bg { get => bg;}

    public void OnInitLayout(GameObject grid)
    {
        this.grid = grid;
        walls = new List<GameObject>(15);
        leftWalls = new List<GameObject>(15);
        bottoWalls = new List<GameObject>(20);
        slots = new List<GameObject>(36);
        CreateUIRoot(ResManager.Instance.uiRootPrefab);
        CreateBG(ResManager.Instance.slotBGPrefab);
        CreteSlotBG(grid);
        CreateWall(ResManager.Instance.wall);
        CreateButtomWall();
    }

    void CreateUIRoot(UIRoot uiRootPrefab)
    {
        uiRoot = GameObject.Instantiate(uiRootPrefab);
    }

    void CreateBG(GameObject slotBGPrefab)
    {
        bg = GameObject.Instantiate(slotBGPrefab);
    }

    void CreteSlotBG(GameObject grid)
    {
        GameObject slot;
        Vector3 startPos = GameCfg.startPoss[GameCfg.level - 1];
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.col; j++)
            {
                slot = CreateFactory.Instance.CreateGameObj<GameObject>(GameObjEunm.slot);
                slot.transform.SetParent(grid.transform);
                slot.transform.position = startPos + new Vector3(.68f * i, -.64f * j, 0);
                slots.Add(slot);
            }
        }
    }

    //生成底下的墙
    void CreateButtomWall()
    {
        Vector3 pos = GameCfg.buttomWallStartPos[GameCfg.level - 1];
        int num = GameCfg.buttomWallNum[GameCfg.level - 1];
        //从左到右依次生成底部的墙
        for (int i = 0; i < num; i++)
        {
            GameObject bw = CreateFactory.Instance.CreateGameObj<GameObject>(GameObjEunm.bottomWall);
            bottoWalls.Add(bw);
            bw.transform.SetParent(bg.transform,false);
            bw.transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }
    }

    //生成两边的墙
    void CreateWall(GameObject wall)
    {
        Vector3 pos = GameCfg.wall[GameCfg.level-1,0];
        //生成左边的墙
        for (int i = 0; i < 15; i++)
        {
            leftWalls.Add(GameObject.Instantiate(wall, pos, Quaternion.identity, bg.transform));
            pos += new Vector3(0,.28f, 0);
        }
        pos = GameCfg.wall[GameCfg.level - 1,1];
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
        GameCfg.level++;
        if (GameCfg.level == 4)
        {
            GameCfg.level = 1;
        }
        //1.底部的墙块要重新设置
        this.OnResetBottomWall();
        //2.两边墙需要重新设置
        this.OnResetWalls();
        //3.消除的背景墙需要重新设置
        this.OnRestSlotBg();
    }

    /// <summary>
    /// 重置墙体
    /// </summary>
    void OnResetWalls()
    {
        Vector3 pos = GameCfg.wall[GameCfg.level - 1, 0];
        //生成左边的墙
        for (int i = 0; i < 15; i++)
        {
            leftWalls[i].transform.position = pos;
            pos += new Vector3(0, .28f, 0);
        }
        pos = GameCfg.wall[GameCfg.level - 1, 1];
        //生成右边的墙
        for (int i = 0; i < 15; i++)
        {
            walls[i].SetActive(true);
            walls[i].transform.position = pos;
            pos += new Vector3(0, .28f, 0);
        }
    }

    /// <summary>
    /// 重置底部墙体
    /// </summary>
    void OnResetBottomWall()
    {
        Vector3 pos = GameCfg.buttomWallStartPos[GameCfg.level - 1];
        //此关卡有多少个底部的墙块
        int num = GameCfg.buttomWallNum[GameCfg.level - 1];
        int maxNum = bottoWalls.Count > num ? bottoWalls.Count : num;
        //先将现有的墙块排好位置
        for (int i = 0; i < maxNum; i++)
        {
            bottoWalls[i].transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }

        GameObject g;
        //如果多了的块则回收
        for (int i = bottoWalls.Count - 1; i >= maxNum; i--)
        {
            g = bottoWalls[i];
            g.transform.parent = null;
            PoolManager.Instance.BottomWall.putObjToPool(g);
            bottoWalls.Remove(g);
        }

        //少了的块则再生成
        for (int i = maxNum; i < num; i++)
        {
            g = CreateFactory.Instance.CreateGameObj<GameObject>(GameObjEunm.bottomWall);
            bottoWalls.Add(g);
            g.transform.SetParent(bg.transform, false);
            g.transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }
    }

    /// <summary>
    /// 重置Slot
    /// </summary>
    void OnRestSlotBg()
    {
        GameObject slot;
        Vector3 startPos = GameCfg.startPoss[GameCfg.level - 1];

        int idx = 0;
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.col; j++)
            {
                idx = i * GameCfg.row + j;
                if (slots.Count > idx)
                {
                    slot = slots[idx];
                    slot.transform.position = startPos + new Vector3(.68f * i, -.64f * j, 0);
                }
                else
                {
                    slot = CreateFactory.Instance.CreateGameObj<GameObject>(GameObjEunm.slot);
                    slot.transform.SetParent(grid.transform);
                    slot.transform.position = startPos + new Vector3(.68f * i, -.64f * j, 0);
                    slots.Add(slot);
                }
            }
        }
        //将列表中多余的物体放回到对象池
        for (int i = idx; i < slots.Count; i++)
        {
            slot = slots[i];
            slot.transform.parent = null;
            slot.transform.position = new Vector3(10000, 10000, 0);
            PoolManager.Instance.SlotPool.putObjToPool(slot);
            slots.Remove(slot);
        }
    }

    void OnResetLayput(Vector3 pos,int num,List<GameObject> objs,Pool<GameObject> pool)
    {
        int maxNum = objs.Count > num ? objs.Count : num;
        //先将现有的墙块排好位置
        for (int i = 0; i < maxNum; i++)
        {
            objs[i].transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }

        GameObject g;
        //如果多了的块则回收
        for (int i = objs.Count - 1; i >= maxNum; i--)
        {
            g = objs[i];
            g.transform.parent = null;
            pool.putObjToPool(g);
            objs.Remove(g);
        }

        //少了的块则再生成
        for (int i = maxNum; i < num; i++)
        {
            g = CreateFactory.Instance.CreateGameObj<GameObject>(GameObjEunm.bottomWall);
            objs.Add(g);
            g.transform.SetParent(bg.transform, false);
            g.transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }
    }

    public void OnDestroy()
    {
        grid = null;
        bg = null;
        uiRoot = null;
        walls.Clear();
        leftWalls.Clear();
        bottoWalls.Clear();
        slots.Clear();
    }
}
