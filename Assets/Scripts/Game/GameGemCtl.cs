using System;
using System.Collections.Generic;
using UnityEngine;

public class GameGemCtl
{
    Transform grid;
    public List<GemsItem> gemsItemsCollect; //�洢�������еı�ʯ
    public bool[,] visited;// ���������

    public GameGemCtl(Transform g)
    {
        this.grid = g;
        gemsItemsCollect = new List<GemsItem>();
        visited = new bool[GameCfg.row, GameCfg.col];
    }

    public GemsItem CreateOneGemItem(Vector3 pos, DirEnum dir, Vector2Int idx, bool isCreateBomb = false)
    {
        GemsItem gemItem = ResManager.Instance.CreateGameObj<GemsItem>(GameObjEunm.gemItem);
        gemItem.transform.SetParent(grid.transform);
        gemItem.transform.position = pos;
        if (ResManager.Instance.gemsSprites.Length > 0)
        {
            //��Ҫ����������
            int spriteIdx = Utils.getGemsIdx(Utils.RandomIntVale(0, 10001));
            //�������
            //Utils.RandomIntVale(0, ResManager.Instance.gemsSprites.Length);
            gemItem.OnInitInfo(isCreateBomb ? ResManager.Instance.bombSprite : ResManager.Instance.gemsSprites[spriteIdx], spriteIdx, dir, idx, isCreateBomb);
        }
        gemItem.TweenTOPosition(.3f);
        return gemItem;
    }

    public void MergeGemAndMove(int x, int y)
    {
        int xIdx = x;
        //��g�ǵڶ��е�Ԫ�أ�Ҫ�ӵ����С������н�Ԫ������
        for (int i = x; i > 0; i--)
        {
            //�õ���һ�е�GemItem
            GemsItem g1 = gemsItemsCollect[(GameCfg.row - i) * GameCfg.row + y];
            //���ԭλ������
            gemsItemsCollect[(GameCfg.row - i) * GameCfg.row + y] = null;
            //���õ���GemItem��ֵ����һ��
            gemsItemsCollect[(GameCfg.row - i - 1) * GameCfg.row + y] = g1;
            g1.Idx = new Vector2Int(xIdx, y);
            //��GemItem��������һ��
            g1.TweenTOPosition();
            xIdx--;
        }
    }

    /// <summary>
    /// ����ը��
    /// </summary>
    /// <param name="curPos"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void CreateBomb(Vector3 curPos, int x, int y,Vector3 tarPos,Action<MergeInfo> cb)
    {
        Bomb b = ResManager.Instance.CreateGameObj<Bomb>(GameObjEunm.bomb);
        b.OnInitInfo(new MergeInfo { row = x, col = y },tarPos , ResManager.Instance.bombSprite, cb, true);
    }

    /// <summary>
    /// ���ƥ������ݺͱ������
    /// </summary>
    public void ClearMatchAndFlag()
    {
        System.Array.Clear(this.visited, 0, this.visited.Length);
    }

    public GemsItem GetGemItem(int x, int y)
    {
        return this.gemsItemsCollect[(GameCfg.row - x - 1) * GameCfg.row + y];
    }

    public List<GemsItem> FindMatches(GemsItem startGem)
    {
        List<GemsItem> matches = new List<GemsItem>();
        Queue<GemsItem> queue = new Queue<GemsItem>();
        int targetType = startGem.GemType;

        // ��ʼ������
        queue.Enqueue(startGem);
        visited[startGem.Idx.x, startGem.Idx.y] = true;

        while (queue.Count > 0)
        {
            GemsItem current = queue.Dequeue();
            matches.Add(current);

            foreach (Vector2Int dir in Utils.directions)
            {
                Vector2Int next = current.Idx + dir;

                // �߽���
                if (next.x < 0 || next.x >= GameCfg.row) continue;
                if (next.y < 0 || next.y >= GameCfg.col) continue;
                /*
                    0123
                   0$$$$
                   1$$$$
                   2$$$$
                   3$$$$
                 */
                GemsItem g = gemsItemsCollect[(GameCfg.row - next.x - 1) * GameCfg.row + next.y];
                // ���ͼ�� && ���ʱ�� 
                if (!visited[next.x, next.y] && (g.GemType & targetType) != 0)
                {
                    visited[next.x, next.y] = true;
                    queue.Enqueue(g);
                }
            }
        }
        return matches.Count >= 4 ? matches : null;
    }

    public void OnRestInfo()
    {
        this.visited = new bool[GameCfg.row, GameCfg.col];
    }

    /// <summary>
    /// ������б�ʯ
    /// </summary>
    public void GemsClear()
    {
        GemsItem g;
        for (int i = 0; i < this.gemsItemsCollect.Count; i++)
        {
            g = this.gemsItemsCollect[i];
            g.IsFull = true;
        }
        //�������֮��Ҫ����ⲿ�ֱ�ʯ
        this.gemsItemsCollect.Clear();
    }
}
