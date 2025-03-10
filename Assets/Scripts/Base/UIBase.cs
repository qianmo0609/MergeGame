using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public UIButton btnClose;

    private void Awake()
    {
        btnClose.onClick.Add(new EventDelegate(this.Hide));
    }

    public virtual void Show()
    {
        this.gameObject.SetActive(true);
    }

    public virtual void Hide() 
    {
        this.gameObject.SetActive(false);
    }
    public void OnDestroy()
    {
        btnClose.onClick.Clear();
    }
}
