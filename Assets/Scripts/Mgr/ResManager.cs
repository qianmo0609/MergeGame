using System.Collections.Generic;
using UnityEngine;

public class ResManager : Singleton<ResManager>
{
    //这里简单将后续要用到的预制体加载进来
    public UIRoot uiRootPrefab;
    public GameObject slotPrefab;
    public GameObject slotBGPrefab;
    public GameObject wall;
    public GameObject buttomWall;
    public Sprite[] gemsSprites;
    public Sprite[] comboSprites;
    public Sprite bombSprite;
    public EffectItem[] effcts;
    public BombEffctItem effectBomb;
    public EffectTextItem effectTextItems;
    public ScoreListItem scoreListItem;
    public GemsItem gemItemPrefab;
    public Bomb bombItemPrefab;
    public LoopListItem loopListItemPrefab;
    public EffectFlyItem effectFlyItemPrefab;
    public Material customSpriteMat;
    public Dictionary<string, UIBase> uiWinsPrefab;
    public Sprite[] levelSprite;
    public override void OnInit()
    {
        base.OnInit();
        this.Onload();
    }
    #region 加载
    public void Onload()
    {
        uiRootPrefab = Resources.Load<UIRoot>("Res/Prefabs/UI Root");
        effectTextItems = Resources.Load<EffectTextItem>($"Res/Prefabs/EffectText");
        slotPrefab = Resources.Load<GameObject>("Res/Prefabs/Slot");
        slotBGPrefab = Resources.Load<GameObject>("Res/Prefabs/BG");
        wall = Resources.Load<GameObject>("Res/Prefabs/Wall");
        buttomWall = Resources.Load<GameObject>("Res/Prefabs/ButtomWall");
        effectBomb = Resources.Load<BombEffctItem>("Res/Prefabs/Effect/elem_bomb_0");
        scoreListItem = Resources.Load<ScoreListItem>("Res/Prefabs/ScoreItem");
        gemItemPrefab = Resources.Load<GemsItem>("Res/Prefabs/GemItem");
        bombSprite = Resources.Load<Sprite>("Res/lhdb/lhdb_ui_gems/gem_bomb");
        customSpriteMat = Resources.Load<Material>("Res/Mat/CustomSpriteClip");
        bombItemPrefab = Resources.Load<Bomb>("Res/Prefabs/Bomb");
        loopListItemPrefab = Resources.Load<LoopListItem>("Res/Prefabs/LoopListItem");
        effectFlyItemPrefab = Resources.Load<EffectFlyItem>("Res/Prefabs/EffectFlyItem");
        this.OnLoadSprite();
        this.OnLoadEffct();
        this.OnLoadComboSprites();
        this.OnLoadLevelSprite();
        this.OnLoadUIWindows();
    }

    void OnLoadSprite()
    {
        gemsSprites = new Sprite[5];
        for (int i = 1; i < 6; i++)
        {
            gemsSprites[i - 1] = Resources.Load<Sprite>($"Res/lhdb/lhdb_ui_gems/gem_{i}");
        }
    }

    void OnLoadEffct()
    {
        effcts = new EffectItem[5];
        for (int i = 1; i < 6; i++)
        {
           effcts[i-1] = Resources.Load<EffectItem>($"Res/Prefabs/Effect/elem_eli_{i}_0");
        }
    }

    void OnLoadComboSprites()
    {
        comboSprites = new Sprite[10];
        for (int i = 0; i < 10; i++)
        {
            comboSprites[i] = Resources.Load<Sprite>($"Res/lhdb/lhdb_font_combo/{i}");
        }
    }

    void OnLoadUIWindows()
    {
        uiWinsPrefab = new Dictionary<string, UIBase>();
        uiWinsPrefab.Add(typeof(RecordUI).Name, Resources.Load<RecordUI>("Res/Prefabs/RecordWin"));
        uiWinsPrefab.Add(typeof(RuleUI).Name, Resources.Load<RuleUI>("Res/Prefabs/RuleWin"));
    }

    void OnLoadLevelSprite()
    {
        levelSprite = new Sprite[3];
        levelSprite[0] = Resources.Load<Sprite>("Res/lhdb/lhdb_ui_main/h5by_xyx_dyg");
        levelSprite[1] = Resources.Load<Sprite>("Res/lhdb/lhdb_ui_main/h5by_xyx_deg");
        levelSprite[2] = Resources.Load<Sprite>("Res/lhdb/lhdb_ui_main/h5by_xyx_dsg");
    }
    #endregion

    #region 实例化
    public T CreateGameObj<T>(GameObjEunm type, int id = 0) where T : class
    {
        switch (type)
        {
            case GameObjEunm.gemItem:
                return PoolManager.Instance.gemsPool.getObjFromPool(type, id) as T;
            case GameObjEunm.effectTextItem:
                return PoolManager.Instance.EffectTextPool.getObjFromPool(type, id) as T;
            case GameObjEunm.effectItem:
                return PoolManager.Instance.EffectItemDic[id].getObjFromPool(type, id) as T;
            case GameObjEunm.effectFlyItem:
                return PoolManager.Instance.EffFlyItemPool.getObjFromPool(type, id) as T;
            case GameObjEunm.loopListItem:
                return PoolManager.Instance.LoopListItemPool.getObjFromPool(type, id) as T;
            case GameObjEunm.scoreListItem:
                return PoolManager.Instance.ScoreListItemPool.getObjFromPool(type, id) as T;
            case GameObjEunm.bomb:
                return PoolManager.Instance.BombItemPool.getObjFromPool(type, id) as T;
            case GameObjEunm.bombEffct:
                return PoolManager.Instance.BombEffctPool.getObjFromPool(type, id) as T;
            case GameObjEunm.slot:
                return PoolManager.Instance.SlotPool.getObjFromPool(type, id) as T;
            case GameObjEunm.bottomWall:
                return PoolManager.Instance.BottomWall.getObjFromPool(type, id) as T;
            default:
                return null;
        }
    }

    public T GetObjPrefab<T>(GameObjEunm type, int id = 0) where T : class
    {
        switch (type)
        {
            case GameObjEunm.gemItem:
                return gemItemPrefab as T;
            case GameObjEunm.bomb:
                return bombItemPrefab as T;
            case GameObjEunm.effectItem:
                return effcts[id - 1] as T;
            case GameObjEunm.effectTextItem:
                return effectTextItems as T;
            case GameObjEunm.effectFlyItem:
                return effectFlyItemPrefab as T;
            case GameObjEunm.loopListItem:
                return loopListItemPrefab as T;
            case GameObjEunm.scoreListItem:
                return scoreListItem as T;
            case GameObjEunm.bombEffct:
                return effectBomb as T;
            case GameObjEunm.bottomWall:
                return buttomWall as T;
            case GameObjEunm.bg:
                return slotBGPrefab as T;
            case GameObjEunm.uiRoot:
                return uiRootPrefab as T;
            default:
                return null;
        }
    }

    public T InstantiateObj<T>(GameObjEunm type) where T : class
    {
        switch (type)
        {
            case GameObjEunm.bottomWall:
                return GameObject.Instantiate<GameObject>(buttomWall) as T;
            case GameObjEunm.slot:
                return GameObject.Instantiate<GameObject>(slotPrefab) as T;
            case GameObjEunm.bg:
                return GameObject.Instantiate<GameObject>(slotBGPrefab) as T;
        }
        return null;
    }

    public T InstantiateMonoObj<T>(GameObjEunm type,int id = 0) where T : MonoBehaviour
    {
        return GameObject.Instantiate<T>(this.GetObjPrefab<T>(type,id));
    }

    #endregion

    #region 回收
    public void PutObjToPool<T>(GameObjEunm type, T t, int id = 0) where T : class
    {
        switch (type)
        {
            case GameObjEunm.gemItem:
                PoolManager.Instance.gemsPool.putObjToPool(t as GemsItem);
                break;
            case GameObjEunm.bomb:
                PoolManager.Instance.BombItemPool.putObjToPool(t as Bomb);
                break;
            case GameObjEunm.slot:
                PoolManager.Instance.SlotPool.putObjToPool(t as GameObject);
                break;
            case GameObjEunm.effectItem:
                PoolManager.Instance.EffectItemDic[id].putObjToPool(t as EffectItem);
                break;
            case GameObjEunm.effectTextItem:
                PoolManager.Instance.EffectTextPool.putObjToPool(t as EffectTextItem);
                break;
            case GameObjEunm.effectFlyItem:
                PoolManager.Instance.EffFlyItemPool.putObjToPool(t as EffectFlyItem);
                break;
            case GameObjEunm.loopListItem:
                PoolManager.Instance.LoopListItemPool.putObjToPool(t as LoopListItem);
                break;
            case GameObjEunm.scoreListItem:
                PoolManager.Instance.ScoreListItemPool.putObjToPool(t as ScoreListItem);
                break;
            case GameObjEunm.bombEffct:
                PoolManager.Instance.BombEffctPool.putObjToPool(t as BombEffctItem);
                break;
            case GameObjEunm.bottomWall:
                PoolManager.Instance.BottomWall.putObjToPool(t as GameObject);
                break;
            default:
                break;
        }
    }
    #endregion

    public void OnDestroy()
    {
        Resources.UnloadUnusedAssets();
    }
}
