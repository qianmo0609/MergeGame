using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class GameMgr : MonoBehaviour
{
    GameMap gameMap = null;
    GameObject grid = null;
    Coroutine creategemCoroutine = null;
    Coroutine gemRandomFullCoroutine = null;
    Coroutine gemMergeCoroutione = null;
    Coroutine restartCoroutione = null;

    List<GemsItem> gemsItemsCollect;
    Stack<GemsItem> mergeItemCollect; //���ڴ洢��Ҫ����������

    ScoreList scoreList;

    Dictionary<int,MergeInfo> gemMergeInfos;

    int bombNumEatchRound = 1;
    Stack<GemsItem> bombCollecion;

    private bool[,] visited;// ���������

    #region yiled return ����
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
        gemsItemsCollect = new List<GemsItem>();
        mergeItemCollect = new Stack<GemsItem>();
        gemMergeInfos = new Dictionary<int, MergeInfo>();
        bombCollecion = new Stack<GemsItem>(1);
        visited = new bool[GameCfg.row, GameCfg.col];
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
           �����̵����½ǿ�ʼ����,
            0123
           0$$$$
           1$$$$
           2$$$$
           3$$$$
           �к��д����Ͻǿ�ʼ�����һ�е�һ��
         */
        for (int j = GameCfg.row - 1; j >= 0; j--)
        {
            for (int i = 0; i < GameCfg.col; i++)
            {
                //gemItem = CreateOneGemItem(Utils.GetCurrentPos(j,i), i <= 1 ? DirEnum.left : DirEnum.right, new Vector2Int(j, i));
                GemsItem gemItem = CreateOneGemItem(Utils.GetStartPos(j, i), i <= 1 ? DirEnum.left : DirEnum.right, new Vector2Int(j, i));
                gemsItemsCollect.Add(gemItem);
                yield return ws005;
            }
        }
        yield return ws02;
        //�����걦ʯ��ʼ���
        DetectMergeGems();

        if (this.creategemCoroutine != null)
        {
            StopCoroutine(this.creategemCoroutine);
            this.creategemCoroutine = null;
        }
    }

    GemsItem CreateOneGemItem(Vector3 pos, DirEnum dir, Vector2Int idx,bool isCreateBomb = false)
    {
        GemsItem gemItem = CreateFactory.Instance.CreateGameObj<GemsItem>(GameObjEunm.gemItem);
        gemItem.transform.SetParent(grid.transform);
        gemItem.transform.position = pos;
        if (ResManager.Instance.gemsSprites.Length > 0)
        {
            //��Ҫ����������
            int spriteIdx = Utils.getGemsIdx(Utils.RandomIntVale(0, 10001));
            //�������
            //Utils.RandomIntVale(0, ResManager.Instance.gemsSprites.Length);
            gemItem.OnInitInfo(isCreateBomb?ResManager.Instance.bombSprite:ResManager.Instance.gemsSprites[spriteIdx], spriteIdx, dir, idx,isCreateBomb);
        }
        gemItem.TweenTOPosition(.3f);
        return gemItem;
    }

    /// <summary>
    /// ��ʯ��ʼ�������������
    /// </summary>
    void StartRandomFull()
    {
        if (GameCfg.gameState != GameState.idle) { Debug.Log(ConstValue.tips); return; }
        gemRandomFullCoroutine = StartCoroutine(RandomFull());
        //��������ڹһ�״̬�£�ÿ�ε������Ҫ���õ���ʼ��ť
        GameCfg.isEnableBtnStart = false;
        EventCenter.Instance.ExcuteEvent(EventNum.EnableOrDisableBtnStartEvent);
    }

#if UNITY_EDITOR
    /// <summary>
    /// ����ʹ��
    /// </summary>
    void TestMerge()
    { 
        int row = Random.Range(1, 4);
        int col = Random.Range(0, 4);
        GemsItem gemItem = gemsItemsCollect[(row - 1) * 4 + col];
        //CollectGrid(gemItem);
    }

    /// <summary>
    /// ���Է�������
    /// </summary>
    /// <returns></returns>
    void TestFlyItem()
    {
        this.CreateFlyGemItem(new MergeInfo { row = 0,col = 2,type = 1});
    }

    /// <summary>
    /// �������壨���ã�
    /// </summary>
    void CollectGrid(GemsItem g)
    {
        //����Ҫ����ʯ������ָ��λ�ã���λ�ò����߼�
        MergeGemAndMove(g.Idx.x,g.Idx.y);
        g.PlayMergeEffect();
    }
#endif
    IEnumerator RandomFull(bool isReCreateFGems = true)
    {
        GemsItem g;
        for (int i = 0; i < gemsItemsCollect.Count; i++)
        {
            g = gemsItemsCollect[i];
            g.IsFull = true;
        }
        //��շ�����ʾ
        scoreList.OnRestInfo();
        //�������֮��Ҫ����ⲿ�ֱ�ʯ
        gemsItemsCollect.Clear();
        //����ÿ��ը����
        this.bombNumEatchRound = 1;
        //������ɺ������ɱ�ʯ
        yield return ws08;
        if (isReCreateFGems) { StartCreateGems(); }
        
        if (gemRandomFullCoroutine != null)
        {
            StopCoroutine(gemRandomFullCoroutine);
            gemRandomFullCoroutine = null;
        }
    }

    /// <summary>
    /// ��ⱦʯ,�ڱ�ʯ�������֮��ʼ��ⱦʯʱ���п�������
    /// </summary>
    public void DetectMergeGems()
    {
        //�����Ϸ�����ˣ��Ͳ���Ҫ�����
        if (GameCfg.gameState == GameState.gameOver) return;
        //�����⵽�п�������ı�ʯ��ִ���������
        if (DetectGemsMethod())
        {
            GameCfg.comboNum++;
            gemMergeCoroutione = StartCoroutine(MergeGems());
        }
        else
        {
            //֪ͨUI��ʾCombo�Ĵ���
            EventCenter.Instance.ExcuteEvent(EventNum.ComboDisplayNumEvent);
            //û�м�⵽��������ı�ʯ��ϲ�״̬����
            GameCfg.gameState = GameState.idle;
            //����ǹһ�״̬,��û�п�����������ʱ������
            if (GameCfg.isHandUp)
            {
                //���û�м�⵽���Ժϲ��ı�ʯ����ȫ����ʯ���䣬�����������б�ʯ
                StartRandomFull();
            }
            else
            {
                //������ǹһ�״̬������Ҫ����StartBtn
                GameCfg.isEnableBtnStart = true;
                EventCenter.Instance.ExcuteEvent(EventNum.EnableOrDisableBtnStartEvent);
            }
        }
    }

    /// <summary>
    /// ���ʱ����������ı�ʯ����
    /// </summary>
    /// <returns></returns>
    public bool DetectGemsMethod()
    {
        bool isMatch = false;
        #region ������ⷨ
        //GemsItem g1, g2, g3;
        ////������
        //for (int i = 0; i < GameCfg.row; i++)
        //{
        //    for (int j = 0; j < GameCfg.row - 2; j++)
        //    {
        //        //��Ϊ�洢�Ǵ����½ǿ�ʼ�洢�ģ����Դ�ͷ������oK��
        //        g1 = gemsItemsCollect[GameCfg.row * i + j];
        //        g2 = gemsItemsCollect[GameCfg.row * i + j + 1];
        //        g3 = gemsItemsCollect[GameCfg.row * i + j + 2];
        //        if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[4 * i + j + 3].GemType) != 0)
        //        {
        //            //��ô˵�����������ʯ��������һ����
        //            mergeItemCollect.Push(g1);
        //            mergeItemCollect.Push(g2);
        //            mergeItemCollect.Push(g3);

        //            this.AddMergeInfo(g1.GemType, 100, g1.Type, g1.Idx.x, g1.Idx.y, 3);
        //            isMatch = true;
        //        }
        //    }
        //}
        //int num = 0;
        ////������
        //for (int i = 0; i < GameCfg.row; i++)
        //{
        //    for (int j = 0; j < GameCfg.row - 2; j++)
        //    {
        //        num = 0;
        //        //��Ϊ�洢�Ǵ����½ǿ�ʼ�洢�ģ����Դ�ͷ������oK��
        //        g1 = gemsItemsCollect[i + j * GameCfg.row];
        //        g2 = gemsItemsCollect[i + (j + 1) * GameCfg.row];
        //        g3 = gemsItemsCollect[i + (j + 2) * GameCfg.row];
        //        if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[i + j * 4 + 3].GemType) != 0)
        //        {
        //            //��ô˵�����������ʯ��������һ����
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

        #region ���������㷨
        /*
         * �����3��������������ô�Ի������ڵĴ�СΪ5*5�ģ��ڶ��μ��Ϳ�������ǰ����Ԫ�أ�
         * ֱ���������ĸ�Ԫ�ؿ�ʼ���,��Ϊ������û��Ҫ�����
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

    // �ķ����������ϡ��¡�����
    Vector2Int[] directions = {
        new Vector2Int(-1,0), //��
        new Vector2Int(1,0), //��
        new Vector2Int(0,-1), //�� 
        new Vector2Int(0,1) //��
    };

    List<GemsItem> matches = new List<GemsItem>();
    Queue<GemsItem> queue = new Queue<GemsItem>();
    List<GemsItem> FindMatches(GemsItem startGem)
    {
        matches.Clear();
        queue.Clear();
        int targetType = startGem.GemType;

        // ��ʼ������
        queue.Enqueue(startGem);
        visited[startGem.Idx.x, startGem.Idx.y] = true;

        while (queue.Count > 0)
        {
            GemsItem current = queue.Dequeue();
            matches.Add(current);

            foreach (Vector2Int dir in directions)
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

    HashSet<GemsItem> allMatches = new HashSet<GemsItem>();
    public bool CheckAllMatches()
    {
        // ���÷��ʱ��
        System.Array.Clear(visited, 0, visited.Length);
        allMatches.Clear();

        GemsItem g;
        // ������������
        for (int x = 0; x < GameCfg.row; x++)
        {
            for (int y = 0; y < GameCfg.col; y++)
            {
                if (!visited[x, y])
                {
                    var matches = FindMatches(gemsItemsCollect[x * GameCfg.row + y]);
                    if (matches != null)
                    {
                        g = matches[0];
                        this.AddMergeInfo(g.GemType, 100, g.Type, g.Idx.x, g.Idx.y, matches.Count);
                        //�����ܷ���
                        GameCfg.totalScore += matches.Count * 100;
                        //֪ͨUI���·���
                        EventCenter.Instance.ExcuteEvent(EventNum.UpdateTotalScoreEvent);
                        foreach (var pos in matches)
                        {
                            allMatches.Add(pos);
                        }
                    }
                }
            }
        }
        return allMatches.Count > 0;
    }

    void AddMergeInfo(int gemType, int score, int type, int row, int col,int num)
    {
        MergeInfo mergeInfo;
        if(gemMergeInfos.TryGetValue(gemType,out mergeInfo))
        {
            mergeInfo.score += score;
            mergeInfo.row = row;
            mergeInfo.col = col;
            mergeInfo.num += num;
        }
        else 
        {
            mergeInfo = new MergeInfo {type = type,score = score,row = row,col = col,num = num};
            gemMergeInfos.Add(gemType, mergeInfo);
        }
    }

    /// <summary>
    /// �����ʯ
    /// </summary>
    IEnumerator MergeGems()
    {
        //GemsItem gemsItem;
        //����һ������ɵ��Աߣ�����һ��������Ч����
        Dictionary<int, MergeInfo>.ValueCollection merges = gemMergeInfos.Values;
        foreach (var item in merges)
        {
            EffectManager.Instance.CreateEffectTextItem(item.score, Utils.GetNGUIPos(item.row), gameMap.UiRoot.transform);
            this.CreateFlyGemItem(item);
            yield return new WaitForSeconds(.1f);
        }
        //����������ϵ�gem�Ͳ�����Ч
        //foreach (var item in mergeItemCollect)
        //{
        //    item.PlayMergeEffect();
        //}
        foreach (var item in allMatches)
        {
            item.PlayMergeEffect();
            //gemsItemsCollect[(GameCfg.row - item.Idx.x - 1) * GameCfg.row + item.Idx.y] = null;
        }
        yield return new WaitForSeconds(.5f);
        //����������
        //while (mergeItemCollect.Count > 0)
        //{
        //    gemsItem = mergeItemCollect.Pop();
        //    MergeGemAndMove(gemsItem.Idx.x, gemsItem.Idx.y);
        //    gemsItem.RecycleSelf();
        //}
        foreach (var item in allMatches)
        {
            MergeGemAndMove(item.Idx.x, item.Idx.y);
            //MergeGemAndMove_(item.Idx.x, item.Idx.y);
            item.RecycleSelf();
        }

        //�������ĸ����͵ķ�����Ϣ
        gemMergeInfos.Clear();

        //�������ը����,�ȴ���ը��
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
        //�������֮���ٽ��м��
        DetectMergeGems();
    }

    //����һ����������ɵ��Ա�
    void CreateFlyGemItem(MergeInfo mergeInfo)
    {
        //���ӱ������ݼ�¼
        LocalData.Instance.AddScoreData(new ScoreData { type = mergeInfo.type, num = mergeInfo.num});
        //���Ӵ�һ����������ɵ�ָ��λ��
        scoreList.AddItem(mergeInfo);
    }

    void MergeGemAndMove(int x, int y) 
    {
        int xIdx = x;
        //��g�ǵڶ��е�Ԫ�أ�Ҫ�ӵ����С������н�Ԫ������
        //Sequence sequence = DOTween.Sequence();
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
            //sequence.Join(g1.TweenTOPosition());
            g1.TweenTOPosition();
            xIdx--;
        }
        //sequence.Play();
        bool isCreateBomb = false;
        //�ȼ���ը�����ʣ��Ƿ���Ҫ����ը��
        if (this.CalacBombPercentage())
        {
            //���С��30����˵���������ɵ���ը��
            //this.CreateBomb(Utils.GetCurrentPos(0, y), 0, y);
            isCreateBomb = true;
            //�����ɵ�ը������1
            this.bombNumEatchRound--;
        }
        //�����µ�GemsItem������λ��
        this.ReplenishGem(0, y, Utils.GetStartPos(0, y), isCreateBomb);
    }

    void CreateBomb(Vector3 curPos,int x, int y)
    {
        Bomb b = CreateFactory.Instance.CreateGameObj<Bomb>(GameObjEunm.bomb);
        b.OnInitInfo(new MergeInfo { row = x,col = y}, gameMap.GetCurrentWallPos(), ResManager.Instance.bombSprite, this.BombCb,true);
    }

    /// <summary>
    /// ը��ִ�����Ļص�����
    /// </summary>
    void BombCb(MergeInfo mergeInfo)
    {
        this.MergeGemAndMove(mergeInfo.row, mergeInfo.col);
        if (gameMap.DestroyWall())
        {
            GameCfg.gameState = GameState.gameOver;
            restartCoroutione = StartCoroutine(this.OnRestartGame());
        }
    }

    IEnumerator OnRestartGame()
    {
        //��������Ϸ���ڿ���״̬��Ҫ�ȴ�
        //�ȴ��ϱ�������
        yield return new WaitForSeconds(1f);
        //������Ҫ����Ϸ״̬��Ϊ��Ϸ����״̬
        GameCfg.level++;
        if (GameCfg.level == 4)
        {
            GameCfg.level = 1;
        }
        Vector2Int ly = GameCfg.gameLayout[GameCfg.level - 1];
        GameCfg.row = ly.x;
        GameCfg.col = ly.y;
        visited = new bool[GameCfg.row, GameCfg.col];
        //���ש��û�ˣ����л�����һ���ؿ�����
        scoreList.OnRestInfo();
        gemRandomFullCoroutine = StartCoroutine(RandomFull(false));
        yield return ws10;
        //���²��ֵ�ͼ
        gameMap.OnRecreate();
        yield return ws20;
        this.StartCreateGems();
        if(restartCoroutione != null)
        {
            StopCoroutine(restartCoroutione);
            restartCoroutione = null;
        }
    }

    /// <summary>
    /// ����ը������
    /// </summary>
    bool CalacBombPercentage()
    {
        //��Ҫ��������µ�Gem����ը��������ǲ���ը��������Ҫ������ը���������µ�Gem���²�����ը��ĿǰĬ�����ڵ�0�У�����������λ������Ҫ�޸Ĵ�ʱ����ֵ
        if (this.bombNumEatchRound > 0 &&  Utils.RandomFloatVale(0.0f, GameCfg.bombPercentageDenominator) < GameCfg.bombPercentageNumerator)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// �����µ�Gem
    /// </summary>
    void ReplenishGem(int x, int y, Vector3 curPos, bool isCreateBomb = false)
    {
        GemsItem gNew = CreateOneGemItem(curPos, y < 3 ? DirEnum.left : DirEnum.right, new Vector2Int(0, y),isCreateBomb);
        gemsItemsCollect[GameCfg.row * (GameCfg.row - 1) + y] = gNew;
        //�����ը�������
        if (isCreateBomb)
        {
            bombCollecion.Push(gNew);
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
