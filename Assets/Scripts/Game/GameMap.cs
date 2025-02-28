using UnityEngine;

public class GameMap
{
    GameObject grid = null;
    GameObject bg = null;
    UIRoot uiRoot = null;

    public UIRoot UiRoot { get => uiRoot;}
    public GameObject Bg { get => bg;}

    public void OnInitLayout(GameObject grid)
    {
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

    //暂时做4*4棋盘
    void CreteSlotBG(GameObject slotPrefab, GameObject grid)
    {
        Vector3 startPos = new Vector3(-1f, 0.03f, 0);
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
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
        for (int i = 0; i < 16; i++)
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
            GameObject.Instantiate(wall, pos, Quaternion.identity, bg.transform);
            pos += new Vector3(0, .28f, 0);
        }
    }

    public void OnDestroy()
    {
        grid = null;
        bg = null;
        uiRoot = null;
    }
}
