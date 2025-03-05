using System;
using UnityEngine;

public class FullComponent
{
    float vv = 3; //重力方向速度
    float vh = 0; //水平方向速度

    float a = 0;  //加速度
    float aa = 0; //加速度的加速度

    Transform ctlObj = null;

    public FullComponent(Transform ctlObj)
    {
        this.ctlObj = ctlObj;        
    }

    public void UpdateInfo()
    {
        vh = Utils.RandomFloatVale(-2, 2);
        a = 20;
        aa = 50;
        vv = Utils.RandomFloatVale(5, 8);
    }

    public void Update(Action cb)
    {
        if (this.ctlObj == null) return;
        vv -= a * Time.deltaTime;
        a = Mathf.Min(50, a + aa * Time.deltaTime);
        this.ctlObj.transform.position += new Vector3(vh, vv, 0) * Time.deltaTime;

        if (this.ctlObj.transform.position.y < -10)
        {
            cb?.Invoke();
        }
    }
}
