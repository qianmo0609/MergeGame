using System;
using UnityEngine;

/// <summary>
/// ���ɵ�ը��װ��
/// </summary>
public class Bomb : EffectFlyItem
{
    Action cb;
    Action<int, Vector3> ReplenishGemsEvent;
    int y;
    Vector3 tarPos;
    public override void OnInitInfo(Vector3 pos, Vector3 tartPos, Sprite sprite, Action cb)
    {
        this.cb = cb;
        this.tarPos = tartPos;
        base.OnInitInfo(pos, tartPos, sprite, this.CallBack);
    }

    public void OnSetInfo(int y , Action<int,Vector3> aciton)
    {
        this.y = y;
        this.ReplenishGemsEvent = aciton;
    }

    public void CallBack()
    {
        //���ɱ�ը��Ч
        EffectManager.Instance.CreateEffectbomb(-1,this.tarPos);
        this.ReplenishGemsEvent?.Invoke(this.y, Utils.GetCurrentPos(0, y));
        cb();
    }
}
