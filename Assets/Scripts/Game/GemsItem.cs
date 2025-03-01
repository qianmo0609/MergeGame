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

    public int GemType { get => gemType; }
    public DirEnum _DirEnum { get => dirEnum; }
    public Vector2Int Idx { get => idx; set => idx = value; }
    public int Type { get => type;}

    Vector3 currentPos;

    public void OnInitInfo(Sprite gemIcon,int type,DirEnum dirEnum,Vector2Int idx)
    {
        this.spriteRenderer = this.GetComponent<SpriteRenderer>();
        this.spriteRenderer.sprite = gemIcon;
        this.spriteRenderer.sortingOrder = 3;
        this.gemType = 1 << type;
        this.type = type;
        this.dirEnum = dirEnum;
        this.idx = idx;
    }

    public Tween TweenTOPosition()
    {
        currentPos = Utils.GetNextPos(this.idx.x,this.idx.y);
        return this.transform.DOMove(currentPos, 0.2f).SetEase(Ease.OutBounce);
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
        this.transform.parent = null;
        this.idx = Vector2Int.down;
        PoolManager.Instance.gemsPool.putObjToPool(this);
    }
}
