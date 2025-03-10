using System;
using System.Collections.Generic;
using UnityEngine;

public class GameGemCtl
{
    Transform grid;
    public List<GemsItem> gemsItemsCollect; //存储盘面所有的宝石
    public bool[,] visited;// 检测标记数组

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
            //需要按概率生成
            int spriteIdx = Utils.getGemsIdx(Utils.RandomIntVale(0, 10001));
            //随机生成
            //Utils.RandomIntVale(0, ResManager.Instance.gemsSprites.Length);
            gemItem.OnInitInfo(isCreateBomb ? ResManager.Instance.bombSprite : ResManager.Instance.gemsSprites[spriteIdx], spriteIdx, dir, idx, isCreateBomb);
        }
        gemItem.TweenTOPosition(.3f);
        return gemItem;
    }

    public void MergeGemAndMove(int x, int y)
    {
        int xIdx = x;
        //如g是第二行的元素，要从第三行、第四行将元素下移
        for (int i = x; i > 0; i--)
        {
            //得到上一行的GemItem
            GemsItem g1 = gemsItemsCollect[(GameCfg.row - i) * GameCfg.row + y];
            //清空原位置数据
            gemsItemsCollect[(GameCfg.row - i) * GameCfg.row + y] = null;
            //将得到的GemItem赋值给下一行
            gemsItemsCollect[(GameCfg.row - i - 1) * GameCfg.row + y] = g1;
            g1.Idx = new Vector2Int(xIdx, y);
            //将GemItem滑动到下一行
            g1.TweenTOPosition();
            xIdx--;
        }
    }

    /// <summary>
    /// 生成炸弹
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
    /// 清除匹配的数据和标记数据
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

        // 初始化队列
        queue.Enqueue(startGem);
        visited[startGem.Idx.x, startGem.Idx.y] = true;

        while (queue.Count > 0)
        {
            GemsItem current = queue.Dequeue();
            matches.Add(current);

            foreach (Vector2Int dir in Utils.directions)
            {
                Vector2Int next = current.Idx + dir;

                // 边界检查
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
                // 类型检查 && 访问标记 
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
    /// 清空所有宝石
    /// </summary>
    public void GemsClear()
    {
        GemsItem g;
        for (int i = 0; i < this.gemsItemsCollect.Count; i++)
        {
            g = this.gemsItemsCollect[i];
            g.IsFull = true;
        }
        //掉落完成之后要清除这部分宝石
        this.gemsItemsCollect.Clear();
    }
}
