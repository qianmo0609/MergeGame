using DG.Tweening;
using System;
using UnityEngine;

public enum DirEnum
{
    left,
    right
}
public class GemsItem : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    int gemType;
    int type;
    DirEnum dirEnum;
    [SerializeField]
    Vector2Int idx; //用于保存物体在什么位置 x表示的是行，y表示的是列
    bool isBomb;
    Tween mTween;

    FullComponent fullComponent;

    public int GemType { get => gemType; }
    public DirEnum _DirEnum { get => dirEnum; }
    public Vector2Int Idx { get => idx; set => idx = value; }
    public int Type { get => type;}
    public bool IsBomb { get => isBomb;}
    public bool IsFull { 
        get { return isFull; } 
        set { 
            isFull = value;
            //vh = Utils.RandomFloatVale(-2,2);
            //a = 20;
            //aa = 50;
            //vv = Utils.RandomFloatVale(5,8);
            fullComponent.UpdateInfo();
        } 
    }

    Vector3 currentPos;

    bool isFull = false;
    //float vv = 3;
    //float vh = 0;

    //float a = 0;
    //float aa = 0;

    private void Start()
    {
        fullComponent = new FullComponent(this.transform);
    }

    public void OnInitInfo(Sprite gemIcon, int type, DirEnum dirEnum, Vector2Int idx, bool isBomb = false)
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.spriteRenderer.sprite = gemIcon;
        this.spriteRenderer.sortingOrder = 3;
        this.gemType = 1 << type;
        this.type = type;
        this.dirEnum = dirEnum;
        this.idx = idx;
        this.isBomb = isBomb;
    }

    public void Update()
    {
        if (this.isFull)
        {
            //vv -= a * Time.deltaTime;
            //a = Mathf.Min(50, a + aa * Time.deltaTime);
            //this.transform.position += new Vector3(vh, vv, 0) * Time.deltaTime;

            //if(this.transform.position.y < -10)
            //{
            //    this.isFull = false;
            //    this.RecycleSelf();
            //}
            fullComponent?.Update(this.UpdateCB);
        }   
    }

    void UpdateCB()
    {
        this.isFull = false;
        this.RecycleSelf();
    }

    public Tween TweenTOPosition()
    {
        currentPos = Utils.GetNextPos(this.idx.x,this.idx.y);
        mTween = this.transform.DOMove(currentPos, 0.2f).SetEase(Ease.OutBounce);
        return mTween;
    }

    public void PlayMergeEffect()
    {
        this.transform.position = new Vector3(10000, 10000, 0);
        //播放爆炸特效动画
        EffectManager.Instance.CreateEffectItem(this.type+1, currentPos);
    }

    /// <summary>
    /// 将自己回收到对象池
    /// </summary>
    /// <param name="p"></param>
    public void RecycleSelf()
    {
        if (this.isBomb)
        {
            this.BombRecycleSelf();
        }
        this.transform.parent = null;
        this.idx = Vector2Int.down;
        mTween.Kill();
        PoolManager.Instance.gemsPool.putObjToPool(this);
        this.isFull = false;
    }

    public void BombRecycleSelf()
    {
        this.isBomb = false;
        this.transform.position = new Vector3(10000, 10000, 0);
        this.gameObject.SetActive(true);
    }
}
