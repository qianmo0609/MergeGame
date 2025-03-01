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
    public GameObject[] effcts;
    public GameObject effectBomb;
    public EffectTextItem effectTextItems;
    public ScoreListItem scoreListItem;
    public Material customSpriteMat;
    public Dictionary<string, UIBase> uiWinsPrefab;
    public override void OnInit()
    {
        base.OnInit();
        this.Onload();
    }
    public void Onload()
    {
        uiRootPrefab = Resources.Load<UIRoot>("Res/Prefabs/UI Root");
        effectTextItems = Resources.Load<EffectTextItem>($"Res/Prefabs/EffectText");
        slotPrefab = Resources.Load<GameObject>("Res/Prefabs/Slot");
        slotBGPrefab = Resources.Load<GameObject>("Res/Prefabs/BG");
        wall = Resources.Load<GameObject>("Res/Prefabs/Wall");
        buttomWall = Resources.Load<GameObject>("Res/Prefabs/ButtomWall");
        effectBomb = Resources.Load<GameObject>("Res/Prefabs/Effect/elem_bomb_0");
        scoreListItem = Resources.Load<ScoreListItem>("Res/Prefabs/ScoreItem");
        bombSprite = Resources.Load<Sprite>("Res/lhdb/lhdb_ui_gems/gem_bomb");
        customSpriteMat = Resources.Load<Material>("Res/Mat/CustomSpriteClip");
        this.OnLoadSprite();
        this.OnLoadEffct();
        this.OnLoadComboSprites();

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
        effcts = new GameObject[5];
        for (int i = 1; i < 6; i++)
        {
           effcts[i-1] = Resources.Load<GameObject>($"Res/Prefabs/Effect/elem_eli_{i}_0");
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

    }

    public void OnDestroy()
    {
    }
}
