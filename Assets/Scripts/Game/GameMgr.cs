using DG.Tweening;
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

    List<GemsItem> gemsItemsCollect;
    Stack<GemsItem> mergeItemCollect; //���ڴ洢��Ҫ����������

    ScoreList scoreList;

    Camera uiCamera;

    Dictionary<int,MergeInfo> gemMergeInfos;
    Stack<Bomb> bombCollection;

    int bombNumEatchRound = 1;

    void Awake()
    {
        this.CreateGrid();
        gameMap = new GameMap();
        gemsItemsCollect = new List<GemsItem>();
        mergeItemCollect = new Stack<GemsItem>();
        bombCollection = new Stack<Bomb>();
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
        //�����걦ʯ��ʼ���
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
            //TODO����Ҫ����������
            int spriteIdx = Utils.RandomIntVale(0, ResManager.Instance.gemsSprites.Length);
            gemItem.OnInitInfo(ResManager.Instance.gemsSprites[spriteIdx], spriteIdx, dir, idx);
        }
        gemItem.TweenTOPosition();
        return gemItem;
    }

    /// <summary>
    /// ��ʯ��ʼ�������������
    /// </summary>
    void StartRandomFull()
    {
        if (GameCfg.gameState != GameState.idle) { Debug.Log("����У������ظ������"); return; }
        gemRandomFullCoroutine = StartCoroutine(RandomFull());
        //this.TestMerge();
        //this.TestFlyItem();
        //this.CreateBomb(Utils.GetCurrentPos(0,2),0,2);
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
        for (int i = 0; i < gemsItemsCollect.Count; i++)
        {
            GemsItem g = gemsItemsCollect[i];
            Vector3 tarPos = g.transform.position + this.getDirOffset(g);
            g.transform.DOMove(tarPos, 0.2f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                //��ʯ����
                g.transform.DOMoveY(-10, Utils.RandomFloatVale(0.1f, 0.3f)).SetEase(Ease.InExpo).OnComplete(() => { RecycleObj(g); });
            });
            yield return new WaitForSeconds(0.02f);
        }
        //�������֮��Ҫ����ⲿ�ֱ�ʯ
        gemsItemsCollect.Clear();
        //������ɺ������ɱ�ʯ
        yield return new WaitForSeconds(0.8f);
        if (isReCreateFGems) { StartCreateGems(); }
        
        if (gemRandomFullCoroutine != null)
        {
            StopCoroutine(gemRandomFullCoroutine);
            gemRandomFullCoroutine = null;
        }
    }

    //���ձ�ʯ
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
    /// ��ⱦʯ,�ڱ�ʯ�������֮��ʼ��ⱦʯʱ���п�������
    /// </summary>
    public void DetectMergeGems()
    {
        //������ɵ���ը����Ҫ�Ƚ���ը���Ĵ���
        if(bombCollection.Count > 0)
        {
            while (bombCollection.Count > 0)
            {
                bombCollection.Pop().IsCanMove = true;
            }
            return;
        }
        //����ÿ��ը����
        this.bombNumEatchRound = 1;
        //�����⵽�п�������ı�ʯ��ִ���������
        if (DetectGemsMethod())
        {
            gemMergeCoroutione = StartCoroutine(MergeGems());
        }
        else
        {
            //û�м�⵽��������ı�ʯ��ϲ�״̬����
            GameCfg.gameState = GameState.idle;
            //����ǹһ�״̬,��û�п�����������ʱ������
            if (GameCfg.isHandUp)
            {
                //���û�м�⵽���Ժϲ��ı�ʯ����ȫ����ʯ���䣬�����������б�ʯ
                StartRandomFull();
            }
            //������ǹһ�״̬����ʲô���������ȴ���ҵ��
        }
    }

    /// <summary>
    /// ���ʱ����������ı�ʯ����
    /// </summary>
    /// <returns></returns>
    public bool DetectGemsMethod()
    {
        #region ������ⷨ
        bool isMatch = false;
        GemsItem g1, g2, g3;
        //������
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.row - 2; j++)
            {
                //��Ϊ�洢�Ǵ����½ǿ�ʼ�洢�ģ����Դ�ͷ������oK��
                g1 = gemsItemsCollect[GameCfg.row * i + j];
                g2 = gemsItemsCollect[GameCfg.row * i + j + 1];
                g3 = gemsItemsCollect[GameCfg.row * i + j + 2];
                if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[4 * i + j + 3].GemType) != 0)
                {
                    //��ô˵�����������ʯ��������һ����
                    mergeItemCollect.Push(g1);
                    mergeItemCollect.Push(g2);
                    mergeItemCollect.Push(g3);

                    this.AddMergeInfo(g1.GemType, 100, g1.Type, g1.Idx.x,g1.Idx.y);
                    isMatch = true;
                }
            }
        }
        //������
        for (int i = 0; i < GameCfg.row; i++)
        {
            for (int j = 0; j < GameCfg.row - 2; j++)
            {
                //��Ϊ�洢�Ǵ����½ǿ�ʼ�洢�ģ����Դ�ͷ������oK��
                g1 = gemsItemsCollect[i + j * GameCfg.row];
                g2 = gemsItemsCollect[i + (j + 1) * GameCfg.row];
                g3 = gemsItemsCollect[i + (j + 2) * GameCfg.row];
                if ((g1.GemType & g2.GemType & g3.GemType) != 0)//& gemsItemsCollect[i + j * 4 + 3].GemType) != 0)
                {
                    //��ô˵�����������ʯ��������һ����
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
            mergeInfo.num++;
        }
        else 
        {
            mergeInfo = new MergeInfo {type = type,score = score,row = row,col = col,num = 1};
            gemMergeInfos.Add(gemType, mergeInfo);
        }
    }

    /// <summary>
    /// �����ʯ
    /// </summary>
    IEnumerator MergeGems()
    {
        GemsItem gemsItem;
        //��Ҫ����һ������ɵ��Աߣ�����һ��������Ч����
        this.CreateOneScoreEffectAndFlyGemItem(gemMergeInfos.Values);
        //����������ϵ�gem�Ͳ�����Ч
        foreach (var item in mergeItemCollect)
        {
            item.PlayMergeEffect();
        }
        yield return new WaitForSeconds(.5f);
        //����������
        while (mergeItemCollect.Count>0)
        {
            GemsItem g = mergeItemCollect.Pop();
            MergeGemAndMove(g.Idx.x,g.Idx.y);
            g.RecycleSelf();
        }

        //�������ĸ����͵ķ�����Ϣ
        gemMergeInfos.Clear();
        yield return new WaitForSeconds(.8f);

        if (gemMergeCoroutione != null)
        {
            StopCoroutine(gemMergeCoroutione);
            gemMergeCoroutione = null;
        }

        //�������֮���ٽ��м��
        DetectMergeGems();
    }



    //����һ�����ַ�����һ����������
    void CreateOneScoreEffectAndFlyGemItem(Dictionary<int, MergeInfo>.ValueCollection merges)
    {
        foreach (var item in merges)
        {
            EffectManager.Instance.CreateEffectTextItem(item.score, Utils.GetNGUIPos(item.row), gameMap.UiRoot.transform);
            this.CreateFlyGemItem(item);
        }
    }

    //����һ����������ɵ��Ա�
    void CreateFlyGemItem(MergeInfo mergeInfo)
    {
        //���ӱ������ݼ�¼
        LocalData.Instance.AddScoreData(new ScoreData { type = mergeInfo.type, score = mergeInfo.score});
        //���Ӵ�һ����������ɵ�ָ��λ��
        scoreList.AddItem(mergeInfo);
    }

    void MergeGemAndMove(int x, int y) 
    {
        int xIdx = x;
        //��g�ǵڶ��е�Ԫ�أ�Ҫ�ӵ����С������н�Ԫ������
        Sequence sequence = DOTween.Sequence();
        for (int i = x; i > 0; i--)
        {
            //�����ը����������λ�ã�Ҳ����˵Ҫ�ȼ������
            if (this.CalacBombPercentage())
            {
                //���С��30����˵���������ɵ���ը��
                this.CreateBomb(Utils.GetCurrentPos(i, y), i, y);
                //�����ɵ�ը������1
                this.bombNumEatchRound--;
            }
            else
            {
                //�õ���һ�е�GemItem
                GemsItem g1 = gemsItemsCollect[(GameCfg.row - i) * GameCfg.row + y];
                //���õ���GemItem��ֵ����һ��
                gemsItemsCollect[(GameCfg.row - i - 1) * GameCfg.row + y] = g1;
                g1.Idx = new Vector2Int(xIdx, y);
                //��GemItem��������һ��
                sequence.Join(g1.TweenTOPosition());
                xIdx--;
            }
        }
        //�����µ�GemsItem������λ��
        this.ReplenishGem(0,y, Utils.GetCurrentPos(0, y));
    }

    void CreateBomb(Vector3 curPos,int x, int y)
    {
        //gemsItemsCollect[(GameCfg.row - x - 1) * GameCfg.row + y];
        Bomb b = CreateFactory.Instance.CreateGameObj<Bomb>(GameObjEunm.bomb);
        b.OnInitInfo(new MergeInfo { row = x,col = y}, gameMap.GetCurrentWallPos(), ResManager.Instance.bombSprite, this.BombCb,true);
        bombCollection.Push(b);
    }

    /// <summary>
    /// ը��ִ�����Ļص�����
    /// </summary>
    void BombCb(MergeInfo mergeInfo)
    {
        if (gameMap.DestroyWall())
        {
            //������Ҫ����Ϸ״̬��Ϊ��Ϸ����״̬
            GameCfg.gameState = GameState.gameOver;
            GameCfg.level++;
            GameCfg.level %= 3;
            //���ש��û�ˣ����л�����һ���ؿ�����
            scoreList.OnRestInfo();
            gemRandomFullCoroutine = StartCoroutine(RandomFull(false));
            //���²��ֵ�ͼ
            gameMap.OnRecreate();
        }
        else
        {
            //this.MergeGemAndMove(mergeInfo.row,mergeInfo.col);
        }
    }

    /// <summary>
    /// ����ը������
    /// </summary>
    bool CalacBombPercentage()
    {
        //��Ҫ��������µ�Gem����ը��������ǲ���ը��������Ҫ������ը���������µ�Gem���²�����ը��ĿǰĬ�����ڵ�0�У�����������λ������Ҫ�޸Ĵ�ʱ����ֵ
        if (this.bombNumEatchRound > 1 &&  Utils.RandomFloatVale(0.0f, 100.0f) < 30)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// �����µ�Gem
    /// </summary>
    void ReplenishGem(int x,int y, Vector3 curPos)
    {
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
    public int num;
    public int row;
    public int col;
}
