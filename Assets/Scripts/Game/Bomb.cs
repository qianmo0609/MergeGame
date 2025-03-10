using System;
using System.IO;
using UnityEngine;

/// <summary>
/// ���ɵ�ը��װ��
/// </summary>
public class Bomb : EffectFlyItem
{
    Action<MergeInfo> cb;
    Vector3 tarPos;
    SpriteRenderer spriteRenderer;

    public override void Update()
    {
        base.Update();
        if (this.IsCanMove)
        {
            this.transform.Rotate(Vector3.forward * 360 * Time.fixedDeltaTime,Space.World);
        }
    }

    public override void OnInitInfo(MergeInfo mergeInfo, Vector3 tartPos, Sprite sprite, Action<MergeInfo> cb, bool isMoveAtOnce = false)
    {
        this.cb = cb;
        this.tarPos = tartPos;
        base.OnInitInfo(mergeInfo, tartPos, sprite, this.CallBack, isMoveAtOnce);
    }

    public void CallBack(MergeInfo mergeInfo)
    {
        //���ɱ�ը��Ч
        EffectManager.Instance.CreateEffectbomb(-1,this.tarPos);
        cb(mergeInfo);
    }

    public override void RecycleSelf()
    {
        this.transform.position = new Vector3(10000, 10000, 0);
        //PoolManager.Instance.BombItemPool.putObjToPool(this);
        ResManager.Instance.PutObjToPool<Bomb>(GameObjEunm.bomb,this);
    }
}
