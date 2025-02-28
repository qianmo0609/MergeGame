using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class GameMgr : MonoBehaviour
{
    GameMap gameMap = null;
    GameObject grid = null;
    Coroutine creategemCoroutine = null;
    Coroutine gemRandomFullCoroutine = null;
    Coroutine gemMergeCoroutione = null;

    List<GemsItem> gemsItemsCollect;
    Stack<GemsItem> mergeItemCollect; //用于存储需要消除的物体

    ScoreList scoreList;

    Camera uiCamera;

    Dictionary<int,MergeInfo> gemMergeInfos;

    void Awake()
    {
        this.CreateGrid();
        gameMap = new GameMap();
        gemsItemsCollect = new List<GemsItem>();
        mergeItemCollect = new Stack<GemsItem>();
        gemMergeInfos = new Dictionary<int, MergeInfo>();
        gameMap.OnInitLayout(grid);
        uiCamera = GameObject.FindGameObjectWithTag("UICamera").GetComponent<Camera>();
        scoreList = new ScoreList(gameMap.Bg.transform.Find("ListObj"));
        StartCreateGems();
        EventCenter.Instance.RegisterEvent(EventNum.TestEvent, StartRandomFull);
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
        GameCfg.isMath = true;
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
        for (int j = 3; j >= 0; j--)
        {
            for (int i = 0; i < 4; i++)
            { 
                GemsItem gemItem = CreateOneGemItem(Utils.GetCurrentPos(j,i), i <= 1 ? DirEnum.left : DirEnum.right, new Vector2Int(j, i)); 
                gemsItemsCollect.Add(gemItem);
                yield return new WaitForSeconds(0.1f);
            }
        }
        yield return new WaitForSeconds(0.2f);
        //生成完宝石开始检测
        DetectMergeGems();

        if (this.creategemCoroutine != null)
        {
            StopCoroutine(this.creategemCoroutine);
            this.creategemCoroutine = null;
        }
    }

    GemsItem CreateOneGemItem(Vector3 pos, DirEnum dir, Vector2Int idx)
    {
        GemsItem gemItem = CreateFactory.Instance.CreateGameObj<GemsItem>(GameObjEunm.gemItem);
        gemItem.transform.SetParent(grid.transform);
        gemItem.transform.position = pos;
        if (ResManager.Instance.gemsSprites.Length > 0)
        {
            //TODO：需要按概率生成
            int spriteIdx = Utils.RandomIntVale(0, ResManager.Instance.gemsSprites.Length);
            gemItem.OnInitInfo(ResManager.Instance.gemsSprites[spriteIdx], spriteIdx, dir, idx);
        }
        gemItem.TweenTOPosition();
        return gemItem;
    }

    /// <summary>
    /// 宝石开始随机往上在下落
    /// </summary>
    void StartRandomFull()
    {
        //if (GameCfg.isMath) { Debug.Log("检测中，请勿重复点击！"); return; }
        //gemRandomFullCoroutine = StartCoroutine(RandomFull());
        //this.TestMerge();
        //this.TestFlyItem();
        this.CreateBomb(Utils.GetCurrentPos(0,2),2);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 测试使用
    /// </summary>
    void TestMerge()
    { 
        int row = Random.Range(1, 4);
        int col = Random.Range(0, 4);
        GemsItem gemItem = gemsItemsCollect[(row - 1) * 4 + col];
        CollectGrid(gemItem);
    }

    /// <summary>
    /// 测试飞行物体
    /// </summary>
    /// <returns></returns>
    void TestFlyItem()
    {
        this.CreateFlyGemItem(new MergeInfo { row = 0,col = 2,type = 1});
    }
#endif

    IEnumerator RandomFull()
    {
        for (int i = 0; i < gemsItemsCollect.Count; i++)
        {
            GemsItem g = gemsItemsCollect[i];
            Vector3 tarPos = g.transform.position + this.getDirOffset(g);
            g.transform.DOMove(tarPos, 0.2f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                //宝石下落
                g.transform.DOMoveY(-10, Utils.RandomFloatVale(0.1f,0.3f)).SetEase(Ease.InExpo).OnComplete(()=> { RecycleObj(g); });
            });
            yield return new WaitForSeconds(0.02f);
        }
        //掉落完成之后要清除这部分宝石
        gemsItemsCollect.Clear();
        //下落完成后再生成宝石
        yield return new WaitForSeconds(0.8f);
        StartCreateGems();
        if (gemRandomFullCoroutine != null)
        {
            StopCoroutine(gemRandomFullCoroutine);
            gemRandomFullCoroutine = null;
        }
    }

    //回收宝石
    void RecycleObj(GemsItem g)
    {
        g.RecycleSelf();
    }

    Vector3 getDirOffset(GemsItem g)
    {
        if (g._DirEnum == DirEnum.left)
        {
            return new Vector3(Utils.RandomFloatVale(-1.0f, 0f), Utils.RandomFloatVale(0, 1.0f), 0);
        }
        else
        {
            return new Vector3(Utils.RandomFloatVale(0f, 1.0f), Utils.RandomFloatVale(0, 1.0f), 0);
        }
    }

    /// <summary>
    /// 检测宝石,在宝石下落完成之后开始检测宝石时候有可消除的
    /// </summary>
    public void DetectMergeGems()
    {
        //如果检测到有可以清除的宝石，执行清除方法
        if (DetectGemsMethod())
        {
            MergeGems();
        }
        else
        {
            //如果是挂机状态,在没有可消除的物体时才下落
            if (GameCfg.isHandUp)
            {
                //如果没有检测到可以合并的宝石，则全部宝石下落，重新生成所有宝石
                StartRandomFull();
            }
            //如果不是挂机状态，则什么都不做
            GameCfg.isMath = false;
        }
    }

    /// <summary>
    /// 检测时候可以消除的宝石方法
    /// </summary>
    /// <returns></returns>
    public bool DetectGemsMethod()
    {
        #region 暴力检测法
        bool isMatch = false;
        GemsItem g1, g2, g3;
        //横向检测
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.row - 2; j++)
            {
                //因为存储是从左下角开始存储的，所以从头遍历是oK的
                g1 = gemsItemsCollect[GameCfg.row * i + j];
                g2 = gemsItemsCollect[GameCfg.row * i + j + 1];
                g3 = gemsItemsCollect[GameCfg.row * i + j + 2];
                if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[4 * i + j + 3].GemType) != 0)
                {
                    //那么说明这个三个宝石的类型是一样的
                    mergeItemCollect.Push(g1);
                    mergeItemCollect.Push(g2);
                    mergeItemCollect.Push(g3);

                    this.AddMergeInfo(g1.GemType, 100, g1.Type, g1.Idx.x,g1.Idx.y);
                    isMatch = true;
                }
            }
        }
        //纵向检测
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.row - 2; j++)
            {
                //因为存储是从左下角开始存储的，所以从头遍历是oK的
                g1 = gemsItemsCollect[i + j * GameCfg.row];
                g2 = gemsItemsCollect[i + (j + 1) * GameCfg.row];
                g3 = gemsItemsCollect[i + (j + 2) * GameCfg.row];
                if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[i + j * 4 + 3].GemType) != 0)
                {
                    //那么说明这个三个宝石的类型是一样的
                    if (!mergeItemCollect.Contains(g1))
                    {
                        mergeItemCollect.Push(g1);
                        isMatch = true;
                    }
                    if (!mergeItemCollect.Contains(g2))
                    {
                        mergeItemCollect.Push(g2);
                        isMatch = true;
                    }
                    if (!mergeItemCollect.Contains(g3))
                    {
                        mergeItemCollect.Push(g3);
                        isMatch = true;
                    }
                    this.AddMergeInfo(g1.GemType, 100, g1.Type, g1.Idx.x,g1.Idx.y);
                }
            }
        }
        #endregion

        #region 滑动窗口法

        #endregion
        return isMatch;
    }

    void AddMergeInfo(int gemType, int score, int type, int row, int col)
    {
        MergeInfo mergeInfo;
        if(gemMergeInfos.TryGetValue(gemType,out mergeInfo))
        {
            mergeInfo.score += score;
            mergeInfo.row = row;
            mergeInfo.col = col;
        }
        else 
        {
            mergeInfo = new MergeInfo {type = type,score = score,row = row,col = col};
            gemMergeInfos.Add(gemType, mergeInfo);
        }
    }

    /// <summary>
    /// 清除宝石
    /// </summary>
    void MergeGems()
    {
        //需要生成一个物体飞到旁边，生成一个分数特效文字
        this.CreateOneScoreEffectAndFlyGemItem(gemMergeInfos.Values);
        while (mergeItemCollect.Count > 0)
        {
            CollectGrid(mergeItemCollect.Pop());
        }
        
        //清除缓存的各类型的分数信息
        gemMergeInfos.Clear();

        //整理完成之后再进行检测
        continueDetect = StartCoroutine(ContinueDetect());

        if (gemMergeCoroutione != null)
        {
            StopCoroutine(gemMergeCoroutione);
            gemMergeCoroutione = null;
        }
    }

    /// <summary>
    /// 整理物体
    /// </summary>
    void CollectGrid(GemsItem g)
    {
        //这里要做宝石滑动到指定位置，将位置补等逻辑
        MergeGemAndMove(g);
        g.PlayMergeEffect();
    }

    Coroutine continueDetect = null;
    IEnumerator ContinueDetect()
    {
        yield return new WaitForSeconds(.5f);
        DetectMergeGems();
        if (continueDetect != null)
        {
            StopCoroutine(continueDetect);
            continueDetect = null;
        }
    }

    //生成一个文字分数和一个飞行物体
    void CreateOneScoreEffectAndFlyGemItem(Dictionary<int, MergeInfo>.ValueCollection merges)
    {
        foreach (var item in merges)
        {
            EffectManager.Instance.CreateEffectTextItem(item.score, Utils.GetNGUIPos(item.row), gameMap.UiRoot.transform);
            this.CreateFlyGemItem(item);
        }
    }

    //生成一个飞行物体飞到旁边
    void CreateFlyGemItem(MergeInfo mergeInfo)
    {
        //增加本地数据记录
        LocalData.Instance.AddScoreData(new ScoreData { type = mergeInfo.type, score = mergeInfo.score});
        //增加从一个飞行物体飞到指定位置
        scoreList.AddItem(mergeInfo);
    }

    void MergeGemAndMove(GemsItem g) 
    {
        //将这个物体在棋盘列上的上的物体往下移动
        int x = g.Idx.x; //行标索引值
        int y = g.Idx.y; //列标
        int xIdx = x;
        //如g是第二行的元素，要从第三行、第四行将元素下移
        for (int i = x; i > 0; i--)
        {
            //得到上一行的GemItem
            GemsItem g1 = gemsItemsCollect[(GameCfg.row - i) * GameCfg.row + y];
            //将得到的GemItem赋值给下一行
            gemsItemsCollect[(GameCfg.row - i - 1) * GameCfg.row + y] = g1;
            g1.Idx = new Vector2Int(xIdx, y);
            //将GemItem滑动到下一行
            g1.TweenTOPosition();
            xIdx--; 
        }
        //需要计算产生新的Gem还是炸弹，如果是产生炸弹，则需要先生成炸弹再生成新的Gem
        Vector3 curPos = Utils.GetCurrentPos(0, y);
        if (Utils.RandomFloatVale(0.0f, 100.0f) < 100)
        {
            //如果小于30，则说明本次生成的是炸弹
            this.CreateBomb(curPos, y);
            return;
        }
        //如果先生成炸弹了，则新的Gem需要在炸弹执行完后再生成
        this.ReplenishGems(y,curPos);
    }

    void CreateBomb(Vector3 curPos, int y)
    {
        Bomb b = CreateFactory.Instance.CreateGameObj<Bomb>(GameObjEunm.bomb);
        b.OnInitInfo(curPos, gameMap.GetCurrentWallPos(), ResManager.Instance.bombSprite, this.BombCb);
        b.OnSetInfo(y, ReplenishGems);
    }

    /// <summary>
    /// 炸弹执行完后的回调函数
    /// </summary>
    void BombCb()
    {
        if (gameMap.DestroyWall())
        {
            //TODO:如果砖块没了，则切换到下一个关卡布局
        }
    }

    /// <summary>
    /// 补充上新的Gem
    /// </summary>
    void ReplenishGems(int y,Vector3 curPos)
    {
        //补充新的GemsItem到顶部位置
        GemsItem gNew = CreateOneGemItem(curPos, y < 3 ? DirEnum.left : DirEnum.right, new Vector2Int(0, y));
        gemsItemsCollect[GameCfg.row * (GameCfg.row - 1) + y] = gNew;
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
    public int row;
    public int col;
}
