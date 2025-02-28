using DG.Tweening;
using System;
using UnityEngine;

public class EffectFlyItem : MonoBehaviour,IFlyComponent
{
    Action cb;

    Vector3 p0, p1, p2;

    bool isCanMove = false;

    // 物体移动的总时间
    public float duration = .3f;
    // 记录开始移动的时间
    private float startTime;

    public virtual void OnInitInfo(Vector3 pos,Vector3 tartPos,Sprite sprite,Action cb)
    {
        SpriteRenderer sp = this.GetComponent<SpriteRenderer>();
        sp.sprite = sprite;
        sp.sortingOrder = 3;
        this.transform.position = pos;
        this.transform.localScale = 0.6856104f * Vector3.one;

        this.cb = cb;
        p0 = pos;
        p1 = new Vector3(pos.x + tartPos.x / 2, pos.y + 2, 0);
        p2 = tartPos;
        isCanMove = true;
        // 记录开始移动的时间
        startTime = Time.time;
        //this.MovePath(pos,tartPos);
    }

    void Update()
    {
        if (isCanMove)
        {
            // 计算当前时间占总时间的比例
            float t = (Time.time - startTime) / duration;
            // 确保t在0到1的范围内
            t = Mathf.Clamp01(t);
            this.transform.position = Utils.CalculateQuadraticBezierPoint(t,p0,p1,p2);
            if (Vector3.Distance(this.transform.position, p2) < 0.01f)
            {
                isCanMove = false;
                this.FlyComplete();
            }
        }
    }

    public void MovePath(Vector3 pos,Vector3 tartPos)
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(this.transform.DOMove(new Vector3(pos.x+tartPos.x/2,pos.y + 2,0),0.3f));
        sequence.Append(this.transform.DOMove(tartPos, 0.3f).SetEase(Ease.OutQuad));
        sequence.Play().OnComplete(this.FlyComplete);
    }

    public void FlyComplete()
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
