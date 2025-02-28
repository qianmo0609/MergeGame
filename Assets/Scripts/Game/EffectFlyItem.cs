using DG.Tweening;
using System;
using UnityEngine;

public class EffectFlyItem : MonoBehaviour,IFlyComponent
{
    Action cb;
    public virtual void OnInitInfo(Vector3 pos,Vector3 tartPos,Sprite sprite,Action cb)
    {
        SpriteRenderer sp = this.GetComponent<SpriteRenderer>();
        sp.sprite = sprite;
        sp.sortingOrder = 3;
        this.transform.position = pos;
        this.transform.localScale = 0.6856104f * Vector3.one;

        this.cb = cb;
        this.MovePath(pos,tartPos);
    }

    public void MovePath(Vector3 pos,Vector3 tartPos)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.transform.DOMove(new Vector3(pos.x+tartPos.x/2,pos.y + 2,0),0.3f));
        sequence.Append(this.transform.DOMove(tartPos, 0.3f).SetEase(Ease.OutQuad));
        sequence.Play().OnComplete(this.Fly);
    }

    public void Fly()
    {
        //飞行完成后，执行回调函数
        this.cb?.Invoke();
        this.RecycleSelf();
    }

    public void RecycleSelf()
    {
        this.transform.position = new Vector3(10000, 10000, 0);
        PoolManager.Instance.EffFlyItemPool.putObjToPool(this);
    }
}
