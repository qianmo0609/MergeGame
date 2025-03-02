using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    Dictionary<string, UIBase> uiCollection;
    public override void OnInit()
    {
        base.OnInit();
        uiCollection = new Dictionary<string, UIBase>();    
    }

    public T GetWindow<T>() where T : UIBase
    {
        string key = typeof(T).Name;
        UIBase win;
        if (uiCollection.TryGetValue(key,out win))
        {
            return win as T;
        }
        else
        {

            if (ResManager.Instance.uiWinsPrefab.TryGetValue(key,out win))
            {
                UIBase ui = GameObject.Instantiate(win);
                uiCollection[key] = ui;
                return ui as T;
            }
            else
            {
                Debug.LogError("查询的窗口不存在！");
                return null;
            }
        }
    }

    public void OnDestroy()
    {
        uiCollection?.Clear();
    }
}
