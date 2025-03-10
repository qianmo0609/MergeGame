using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    GameMap gameMap = null;
    GameObject grid = null;
    Coroutine creategemCoroutine = null;
    Coroutine gemRandomFullCoroutine = null;
    Coroutine gemMergeCoroutione = null;
    Coroutine restartCoroutione = null;

    ScoreList scoreList;
    List<MergeInfo> gemMergeInfos;
    public List<List<GemsItem>> allMatches; //用于存储需要消除的物体

    int bombNumEatchRound = 1; //炸弹数量

    Stack<GemsItem> bombCollecion;

    GameGemCtl gemCtl;

    #region yiled return 定义
    WaitForSeconds ws005;
    WaitForSeconds ws01;
    WaitForSeconds ws02;
    WaitForSeconds ws03;
    WaitForSeconds ws05;
    WaitForSeconds ws08;
    WaitForSeconds ws10;
    WaitForSeconds ws20;
    #endregion

    void Awake()
    {
        this.CreateGrid();
        this.CreateWS();
        gameMap = new GameMap();
        gemCtl = new GameGemCtl(this.grid.transform);
        gemMergeInfos = new List<MergeInfo>();
        bombCollecion = new Stack<GemsItem>(1);
        allMatches = new List<List<GemsItem>>();

        gameMap.OnInitLayout(grid);
        scoreList = new ScoreList(gameMap.Bg.transform.Find("ListObj"));
        StartCreateGems();
        EventCenter.Instance.RegisterEvent(EventNum.TestEvent, StartRandomFull);
    }

    void CreateWS()
    {
        ws005 = new WaitForSeconds(.015f);
        ws01 = new WaitForSeconds(.1f);
        ws02 = new WaitForSeconds(.2f);
        ws03 = new WaitForSeconds(.3f);
        ws05 = new WaitForSeconds(.5f);
        ws08 = new WaitForSeconds(.8f);
        ws10 = new WaitForSeconds(1f);
        ws20 = new WaitForSeconds(2f);
    }

    void CreateGrid()
    {
        if (this.grid == null)
            grid = new GameObject("Grid");
    }

    void StartCreateGems()
    {
        creategemCoroutine = StartCoroutine(this.CreateGem());
    }

    IEnumerator CreateGem()
    {
        GameCfg.gameState= GameState.isMatching;
        /*
           $$$$   
           $$$$
           $$$$
         ->$$$$
           从棋盘的左下角开始生成,
            0123
           0$$$$
           1$$$$
           2$$$$
           3$$$$
           行和列从左上角开始计算第一行第一列
         */
        GemsItem gemItem;
        for (int j = GameCfg.row - 1; j >= 0; j--)
        {
            for (int i = 0; i < GameCfg.col; i++)
            {
                gemItem = gemCtl.CreateOneGemItem(Utils.GetStartPos(j, i), i <= 1 ? DirEnum.left : DirEnum.right, new Vector2Int(j, i));
                gemCtl.gemsItemsCollect.Add(gemItem);
                yield return ws005;
            }
        }
        yield return ws02;
        //生成完宝石开始检测
        DetectMergeGems();

        if (this.creategemCoroutine != null)
        {
            StopCoroutine(this.creategemCoroutine);
            this.creategemCoroutine = null;
        }
    }

    /// <summary>
    /// 宝石开始随机往上在下落
    /// </summary>
    void StartRandomFull()
    {
        if (GameCfg.gameState != GameState.idle) { Debug.Log(ConstValue.tips); return; }
        gemRandomFullCoroutine = StartCoroutine(RandomFull());
        //如果不是在挂机状态下，每次点击都需要禁用掉开始按钮
        GameCfg.isEnableBtnStart = false;
        EventCenter.Instance.ExcuteEvent(EventNum.EnableOrDisableBtnStartEvent);
    }

    IEnumerator RandomFull(bool isReCreateFGems = true)
    {
        //清空所有宝石
        gemCtl.GemsClear();
        //清空分数显示
        scoreList.OnRestInfo();
        //重置每轮炸弹数
        this.bombNumEatchRound = 1;
        //下落完成后再生成宝石
        yield return ws08;
        if (isReCreateFGems) { StartCreateGems(); }
        
        if (gemRandomFullCoroutine != null)
        {
            StopCoroutine(gemRandomFullCoroutine);
            gemRandomFullCoroutine = null;
        }
    }

    /// <summary>
    /// 检测宝石,在宝石下落完成之后开始检测宝石时候有可消除的
    /// </summary>
    public void DetectMergeGems()
    {
        //如果游戏结束了，就不需要检测了
        if (GameCfg.gameState == GameState.gameOver) return;
        //如果检测到有可以清除的宝石，执行清除方法
        if (DetectGemsMethod())
        {
            GameCfg.comboNum++;
            gemMergeCoroutione = StartCoroutine(MergeGems());
        }
        else
        {
            //通知UI显示Combo的次数
            EventCenter.Instance.ExcuteEvent(EventNum.ComboDisplayNumEvent);
            //没有检测到可以清除的宝石则合并状态结束
            GameCfg.gameState = GameState.idle;
            //如果是挂机状态,在没有可消除的物体时才下落
            if (GameCfg.isHandUp)
            {
                //如果没有检测到可以合并的宝石，则全部宝石下落，重新生成所有宝石
                StartRandomFull();
            }
            else
            {
                //如果不是挂机状态，则需要开启StartBtn
                GameCfg.isEnableBtnStart = true;
                EventCenter.Instance.ExcuteEvent(EventNum.EnableOrDisableBtnStartEvent);
            }
        }
    }

    /// <summary>
    /// 检测时候可以消除的宝石方法
    /// </summary>
    /// <returns></returns>
    public bool DetectGemsMethod()
    {
        bool isMatch = false;
        #region 暴力检测法
        //GemsItem g1, g2, g3;
        ////横向检测
        //for (int i = 0; i < GameCfg.row; i++)
        //{
        //    for (int j = 0; j < GameCfg.row - 2; j++)
        //    {
        //        //因为存储是从左下角开始存储的，所以从头遍历是oK的
        //        g1 = gemsItemsCollect[GameCfg.row * i + j];
        //        g2 = gemsItemsCollect[GameCfg.row * i + j + 1];
        //        g3 = gemsItemsCollect[GameCfg.row * i + j + 2];
        //        if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[4 * i + j + 3].GemType) != 0)
        //        {
        //            //那么说明这个三个宝石的类型是一样的
        //            mergeItemCollect.Push(g1);
        //            mergeItemCollect.Push(g2);
        //            mergeItemCollect.Push(g3);

        //            this.AddMergeInfo(g1.GemType, 100, g1.Type, g1.Idx.x, g1.Idx.y, 3);
        //            isMatch = true;
        //        }
        //    }
        //}
        //int num = 0;
        ////纵向检测
        //for (int i = 0; i < GameCfg.row; i++)
        //{
        //    for (int j = 0; j < GameCfg.row - 2; j++)
        //    {
        //        num = 0;
        //        //因为存储是从左下角开始存储的，所以从头遍历是oK的
        //        g1 = gemsItemsCollect[i + j * GameCfg.row];
        //        g2 = gemsItemsCollect[i + (j + 1) * GameCfg.row];
        //        g3 = gemsItemsCollect[i + (j + 2) * GameCfg.row];
        //        if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[i + j * 4 + 3].GemType) != 0)
        //        {
        //            //那么说明这个三个宝石的类型是一样的
        //            if (!mergeItemCollect.Contains(g1))
        //            {
        //                mergeItemCollect.Push(g1);
        //                isMatch = true;
        //                num++;
        //            }
        //            if (!mergeItemCollect.Contains(g2))
        //            {
        //                mergeItemCollect.Push(g2);
        //                isMatch = true;
        //                num++;
        //            }
        //            if (!mergeItemCollect.Contains(g3))
        //            {
        //                mergeItemCollect.Push(g3);
        //                isMatch = true;
        //                num++;
        //            }
        //            this.AddMergeInfo(g1.GemType, 100, g1.Type, g1.Idx.x, g1.Idx.y, num);
        //        }
        //    }
        //}
        #endregion

        #region 滑动窗口算法
        /*
         * 如果是3个可以消除，那么以滑动窗口的大小为5*5的，第二次检测就可以跳过前三个元素，
         * 直接跳到第四个元素开始检查,因为这三个没必要检测了
          |$$$$$|$$$$              $$$|$$$$$|$
          |$$$$$|$$$$     --->     $$$|$$$$$|$  
          |$$$$$|$$$$              $$$|$$$$$|$
          |$$$$$|$$$$              $$$|$$$$$|$
          |$$$$$|$$$$              $$$|$$$$$|$
           $$$$$$$$$               $$$$$$$$$
         */
        #endregion

        isMatch = this.CheckAllMatches();
        return isMatch;
    }

    public bool CheckAllMatches()
    {
        // 重置访问标记
        gemCtl.ClearMatchAndFlag();

        GemsItem g;
        // 遍历整个网格
        for (int x = 0; x < GameCfg.row; x++)
        {
            for (int y = 0; y < GameCfg.col; y++)
            {
                if (!gemCtl.visited[x, y])
                {
                    var matches = gemCtl.FindMatches(gemCtl.GetGemItem(x, y));
                    if (matches != null)
                    {
                        g = matches[0];
                        this.AddMergeInfo(100, g.Type, g.Idx.x, g.Idx.y, matches.Count);
                        //计算总分数
                        GameCfg.totalScore += matches.Count * 100;
                        //通知UI更新分数
                        EventCenter.Instance.ExcuteEvent(EventNum.UpdateTotalScoreEvent);
                        this.allMatches.Add(matches);
                    }
                }
            }
        }
        return this.allMatches.Count > 0;
    }

    void AddMergeInfo(int score, int type, int row, int col,int num)
    {
        MergeInfo mergeInfo = new MergeInfo { type = type, score = score, row = row, col = col, num = num };
        gemMergeInfos.Add(mergeInfo);
    }

    /// <summary>
    /// 清除宝石
    /// </summary>
    IEnumerator MergeGems()
    {
        //先清除棋盘上的gem和播放特效
        MergeInfo mergeInfo;
        for (int i = 0; i < this.allMatches.Count; i++)
        {
            foreach (var item in this.allMatches[i])
            {
                item.PlayMergeEffect();
            }
            mergeInfo = gemMergeInfos[i];
            EffectManager.Instance.CreateEffectTextItem(mergeInfo.score, Utils.GetNGUIPos(mergeInfo.row), gameMap.UiRoot.transform);
            this.CreateFlyGemItem(mergeInfo);
            yield return new WaitForSeconds(.5f);
        }
        yield return new WaitForSeconds(.5f);
        //再整理棋盘
        for (int i = 0;i < this.allMatches.Count; i++)
        {
            foreach (var item in this.allMatches[i])
            {
                MergeGemAndMove(item.Idx.x, item.Idx.y);
                item.RecycleSelf();
            }
        }

        //清除缓存的各类型的分数信息
        gemMergeInfos.Clear();

        //如果生成炸弹了,先处理炸弹
        if (bombCollecion.Count > 0)
        {
            GemsItem g = bombCollecion.Pop();
            Vector2Int idx = g.Idx;
            yield return ws03;
            g.gameObject.SetActive(false);
            this.CreateBomb(Utils.GetCurrentPos(idx.x, idx.y), idx.x, idx.y);
        }
        yield return ws08;

        if (gemMergeCoroutione != null)
        {
            StopCoroutine(gemMergeCoroutione);
            gemMergeCoroutione = null;
        }
        //整理完成之后再进行检测
        DetectMergeGems();
    }

    void MergeGemAndMove(int x, int y)
    {
        //移动棋盘上的宝石
        gemCtl.MergeGemAndMove(x,y);
        this.allMatches.Clear();

        //sequence.Play();
        bool isCreateBomb = false;
        //先计算炸弹概率，是否需要生成炸弹
        if (this.CalacBombPercentage())
        {
            //如果小于30，则说明本次生成的是炸弹
            isCreateBomb = true;
            //将生成的炸弹数减1
            this.bombNumEatchRound--;
        }
        //补充新的GemsItem到顶部位置
        this.ReplenishGem(0, y, Utils.GetStartPos(0, y), isCreateBomb);
    }

    /// <summary>
    /// 计算炸弹概率
    /// </summary>
    bool CalacBombPercentage()
    {
        //需要计算产生新的Gem还是炸弹，如果是产生炸弹，则需要先生成炸弹再生成新的Gem，新产生的炸弹目前默认是在第0行，如需在任意位置则需要修改此时行列值
        if (this.bombNumEatchRound > 0 && Utils.RandomFloatVale(0.0f, GameCfg.bombPercentageDenominator) < GameCfg.bombPercentageNumerator)
        {
            return true;
        }
        return false;
    }

    void CreateBomb(Vector3 curPos, int x, int y)
    { 
        gemCtl.CreateBomb(curPos,x,y, gameMap.GetCurrentWallPos(),this.BombCb);
    }

    void BombCb(MergeInfo mergeInfo)
    {
        this.MergeGemAndMove(mergeInfo.row, mergeInfo.col);
        if (gameMap.DestroyWall())
        {
            //如果砖块没了，则切换到下一个关卡
            GameCfg.gameState = GameState.gameOver;
            restartCoroutione = StartCoroutine(this.OnRestartGame());
        }
    }

    /// <summary>
    /// 补充新的Gem
    /// </summary>
    void ReplenishGem(int x, int y, Vector3 curPos, bool isCreateBomb = false)
    {
        GemsItem gNew = gemCtl.CreateOneGemItem(curPos, y < 3 ? DirEnum.left : DirEnum.right, new Vector2Int(0, y), isCreateBomb);
        gemCtl.gemsItemsCollect[GameCfg.row * (GameCfg.row - 1) + y] = gNew;
        //如果是炸弹则添加
        if (isCreateBomb)
        {
            bombCollecion.Push(gNew);
        }
    }

    /// <summary>
    /// 生成一个飞行物体飞到旁边
    /// </summary>
    /// <param name="mergeInfo"></param>
    void CreateFlyGemItem(MergeInfo mergeInfo)
    {
        //增加本地数据记录
        LocalData.Instance.AddScoreData(new ScoreData { type = mergeInfo.type, num = mergeInfo.num });
        //增加从一个飞行物体飞到指定位置
        scoreList.AddItem(mergeInfo);
    }

    IEnumerator OnRestartGame()
    {
        //检测如果游戏不在空闲状态需要等待
        //等待上边整理完
        yield return new WaitForSeconds(1f);
        //首先需要将游戏状态改为游戏结束状态
        GameCfg.level++;
        if (GameCfg.level == 4)
        {
            GameCfg.level = 1;
        }
        Vector2Int ly = GameCfg.gameLayout[GameCfg.level - 1];
        GameCfg.row = ly.x;
        GameCfg.col = ly.y;
        gemCtl.OnRestInfo();
        scoreList.OnRestInfo();
        gemRandomFullCoroutine = StartCoroutine(RandomFull(false));
        yield return ws10;
        //重新布局地图
        gameMap.OnRecreate();
        yield return ws20;
        this.StartCreateGems();
        if(restartCoroutione != null)
        {
            StopCoroutine(restartCoroutione);
            restartCoroutione = null;
        }
    }

    public void OnDestroy_()
    {
        gameMap = null;
        grid = null;
        creategemCoroutine = null;
        gemRandomFullCoroutine = null;
        gemMergeCoroutione = null;
        StopAllCoroutines();
        EventCenter.Instance.UnregisterEvent(EventNum.TestEvent);
    }
}

public struct MergeInfo
{
    public int type;
    public int score;
    public int num;
    public int row;
    public int col;
}
