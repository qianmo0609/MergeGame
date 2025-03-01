using System;
using UnityEngine;

/// <summary>
/// ���ɵ�ը��װ��
/// </summary>
public class Bomb : EffectFlyItem
{
    Action<MergeInfo> cb;
    Vector3 tarPos;
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
}
