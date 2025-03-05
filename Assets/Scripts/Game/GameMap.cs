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

    SpriteRenderer levelSprite;
    int currentWallNum = 0;

    public UIRoot UiRoot { get => uiRoot; }
    public GameObject Bg { get => bg; }

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
        currentWallNum = GameCfg.wallNum;
    }

    void CreateUIRoot(UIRoot uiRootPrefab)
    {
        uiRoot = GameObject.Instantiate(uiRootPrefab);
    }

    void CreateBG(GameObject slotBGPrefab)
    {
        bg = GameObject.Instantiate(slotBGPrefab);
        this.levelSprite = bg.transform.Find("Level").GetComponent<SpriteRenderer>();
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

    //���ɵ��µ�ǽ
    void CreateButtomWall()
    {
        Vector3 pos = GameCfg.buttomWallStartPos[GameCfg.level - 1];
        int num = GameCfg.buttomWallNum[GameCfg.level - 1];
        //�������������ɵײ���ǽ
        for (int i = 0; i < num; i++)
        {
            GameObject bw = CreateFactory.Instance.CreateGameObj<GameObject>(GameObjEunm.bottomWall);
            bottoWalls.Add(bw);
            bw.transform.SetParent(bg.transform, false);
            bw.transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }
    }

    //�������ߵ�ǽ
    void CreateWall(GameObject wall)
    {
        Vector3 pos = GameCfg.wall[GameCfg.level - 1, 0];
        //������ߵ�ǽ
        for (int i = 0; i < 15; i++)
        {
            leftWalls.Add(GameObject.Instantiate(wall, pos, Quaternion.identity, bg.transform));
            pos += new Vector3(0, .28f, 0);
        }
        pos = GameCfg.wall[GameCfg.level - 1, 1];
        //�����ұߵ�ǽ
        for (int i = 0; i < 15; i++)
        {
            walls.Add(GameObject.Instantiate(wall, pos, Quaternion.identity, bg.transform));
            pos += new Vector3(0, .28f, 0);
        }
    }

    public bool DestroyWall()
    {
        walls[currentWallNum - 1].SetActive(false);
        currentWallNum--;
        return currentWallNum <= 0;
    }

    /// <summary>
    /// �õ���ǰש���λ��
    /// </summary>
    /// <returns></returns>
    public Vector3 GetCurrentWallPos()
    {
        return this.grid.transform.TransformPoint(walls[currentWallNum - 1].transform.position);
    }

    /// <summary>
    /// ���²��ֵ�ͼ
    /// </summary>
    public void OnRecreate()
    {
#if UNITY_EDITOR
        //GameCfg.level++;
        //if (GameCfg.level == 4)
        //{
        //    GameCfg.level = 1;
        //}
        //Vector2Int ly = GameCfg.gameLayout[GameCfg.level - 1];
        //GameCfg.row = ly.x;
        //GameCfg.col = ly.y;
#endif
        this.levelSprite.sprite = ResManager.Instance.levelSprite[GameCfg.level - 1];
        //���õ�ǰש��
        currentWallNum = GameCfg.wallNum;
        //1.�ײ���ǽ��Ҫ��������
        this.OnResetBottomWall();
        //2.����ǽ��Ҫ��������
        this.OnResetWalls();
        //3.�����ı���ǽ��Ҫ��������
        this.OnRestSlotBg();
    }

    /// <summary>
    /// ����ǽ��
    /// </summary>
    void OnResetWalls()
    {
        Vector3 pos = GameCfg.wall[GameCfg.level - 1, 0];
        //������ߵ�ǽ
        for (int i = 0; i < 15; i++)
        {
            leftWalls[i].transform.position = pos;
            pos += new Vector3(0, .28f, 0);
        }
        pos = GameCfg.wall[GameCfg.level - 1, 1];
        //�����ұߵ�ǽ
        for (int i = 0; i < 15; i++)
        {
            walls[i].SetActive(true);
            walls[i].transform.position = pos;
            pos += new Vector3(0, .28f, 0);
        }
    }

    /// <summary>
    /// ���õײ�ǽ��
    /// </summary>
    void OnResetBottomWall()
    {
        Vector3 pos = GameCfg.buttomWallStartPos[GameCfg.level - 1];
        //�˹ؿ��ж��ٸ��ײ���ǽ��
        int num = GameCfg.buttomWallNum[GameCfg.level - 1];
        int maxNum = bottoWalls.Count > num ? num : bottoWalls.Count;
        //�Ƚ����е�ǽ���ź�λ��
        for (int i = 0; i < maxNum; i++)
        {
            bottoWalls[i].transform.localPosition = pos;
            pos += new Vector3(.25f, 0, 0);
        }

        GameObject g;
        //������˵Ŀ������
        for (int i = bottoWalls.Count - 1; i >= maxNum; i--)
        {
            g = bottoWalls[i];
            g.transform.parent = null;
            g.transform.position = new Vector3(1000,-1000,0);
            PoolManager.Instance.BottomWall.putObjToPool(g);
            bottoWalls.Remove(g);
        }

        //���˵Ŀ���������
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
    /// ����Slot
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
        //���б��ж��������Żص������
        for (int i = slots.Count - 1; i > idx ; i--)
        {
            slot = slots[i];
            slot.transform.parent = null;
            slot.transform.position = new Vector3(10000, 10000, 0);
            slots.Remove(slot);
            PoolManager.Instance.SlotPool.putObjToPool(slot);
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
