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
    Vector2Int idx; //���ڱ���������ʲôλ�� x��ʾ�����У�y��ʾ������

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

    public void TweenTOPosition()
    {
        currentPos = Utils.GetNextPos(this.idx.x,this.idx.y);
        this.transform.DOMove(currentPos, 0.2f).SetEase(Ease.OutBounce);
    }

    public void PlayMergeEffect()
    {
        this.RecycleSelf();
        //���ű�ը��Ч����
        EffectManager.Instance.CreateEffectItem(UnityEngine.Random.Range(1,6), currentPos);
    }

    /// <summary>
    /// ���Լ����յ������
    /// </summary>
    /// <param name="p"></param>
    public void RecycleSelf()
    {
        this.transform.parent = null;
        this.transform.position = new Vector3(10000, 10000, 0);
        this.idx = Vector2Int.down;
        PoolManager.Instance.gemsPool.putObjToPool(this);
    }
}
