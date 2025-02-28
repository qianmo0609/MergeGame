using System;
using UnityEngine;

public class EffectFlyItem : MonoBehaviour,IFlyComponent
{
    Action cb;
    public void OnInitInfo(Vector3 pos,Vector3 tartPos,Action cb)
    {
        this.transform.position = pos;
        this.cb = cb;
    }

    public void Fly()
    {

        //飞行完成后，执行回调函数
        this.cb?.Invoke();
    }

    public void RecycleSelf()
    {
        this.transform.position = new Vector3(10000, 10000, 0);
        PoolManager.Instance.EffFlyItemPool.putObjToPool(this);
    }
}
