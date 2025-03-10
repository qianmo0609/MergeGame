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
    public List<List<GemsItem>> allMatches; //���ڴ洢��Ҫ����������

    int bombNumEatchRound = 1; //ը������

    Stack<GemsItem> bombCollecion;

    GameGemCtl gemCtl;

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
           �����̵����½ǿ�ʼ����,
            0123
           0$$$$
           1$$$$
           2$$$$
           3$$$$
           �к��д����Ͻǿ�ʼ�����һ�е�һ��
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
        //�����걦ʯ��ʼ���
        DetectMergeGems();

        if (this.creategemCoroutine != null)
        {
            StopCoroutine(this.creategemCoroutine);
            this.creategemCoroutine = null;
        }
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

    IEnumerator RandomFull(bool isReCreateFGems = true)
    {
        //������б�ʯ
        gemCtl.GemsClear();
        //��շ�����ʾ
        scoreList.OnRestInfo();
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

    public bool CheckAllMatches()
    {
        // ���÷��ʱ��
        gemCtl.ClearMatchAndFlag();

        GemsItem g;
        // ������������
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
                        //�����ܷ���
                        GameCfg.totalScore += matches.Count * 100;
                        //֪ͨUI���·���
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
    /// �����ʯ
    /// </summary>
    IEnumerator MergeGems()
    {
        //����������ϵ�gem�Ͳ�����Ч
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
        //����������
        for (int i = 0;i < this.allMatches.Count; i++)
        {
            foreach (var item in this.allMatches[i])
            {
                MergeGemAndMove(item.Idx.x, item.Idx.y);
                item.RecycleSelf();
            }
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

    void MergeGemAndMove(int x, int y)
    {
        //�ƶ������ϵı�ʯ
        gemCtl.MergeGemAndMove(x,y);
        this.allMatches.Clear();

        //sequence.Play();
        bool isCreateBomb = false;
        //�ȼ���ը�����ʣ��Ƿ���Ҫ����ը��
        if (this.CalacBombPercentage())
        {
            //���С��30����˵���������ɵ���ը��
            isCreateBomb = true;
            //�����ɵ�ը������1
            this.bombNumEatchRound--;
        }
        //�����µ�GemsItem������λ��
        this.ReplenishGem(0, y, Utils.GetStartPos(0, y), isCreateBomb);
    }

    /// <summary>
    /// ����ը������
    /// </summary>
    bool CalacBombPercentage()
    {
        //��Ҫ��������µ�Gem����ը��������ǲ���ը��������Ҫ������ը���������µ�Gem���²�����ը��ĿǰĬ�����ڵ�0�У�����������λ������Ҫ�޸Ĵ�ʱ����ֵ
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
            //���ש��û�ˣ����л�����һ���ؿ�
            GameCfg.gameState = GameState.gameOver;
            restartCoroutione = StartCoroutine(this.OnRestartGame());
        }
    }

    /// <summary>
    /// �����µ�Gem
    /// </summary>
    void ReplenishGem(int x, int y, Vector3 curPos, bool isCreateBomb = false)
    {
        GemsItem gNew = gemCtl.CreateOneGemItem(curPos, y < 3 ? DirEnum.left : DirEnum.right, new Vector2Int(0, y), isCreateBomb);
        gemCtl.gemsItemsCollect[GameCfg.row * (GameCfg.row - 1) + y] = gNew;
        //�����ը�������
        if (isCreateBomb)
        {
            bombCollecion.Push(gNew);
        }
    }

    /// <summary>
    /// ����һ����������ɵ��Ա�
    /// </summary>
    /// <param name="mergeInfo"></param>
    void CreateFlyGemItem(MergeInfo mergeInfo)
    {
        //���ӱ������ݼ�¼
        LocalData.Instance.AddScoreData(new ScoreData { type = mergeInfo.type, num = mergeInfo.num });
        //���Ӵ�һ����������ɵ�ָ��λ��
        scoreList.AddItem(mergeInfo);
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
        gemCtl.OnRestInfo();
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
