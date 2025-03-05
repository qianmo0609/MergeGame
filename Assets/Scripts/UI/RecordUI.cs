using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordUI : UIBase
{
    public UIScrollView scrollView;
    public UIGrid grid;
    public LoopListItem itemPrefab;
    public UIPanel uiPanel;
    public float itemHeight;  // 列表项的高度

    LoopList loopList;

    private void OnEnable()
    {
        loopList = new LoopList(scrollView, grid, itemPrefab, uiPanel, itemHeight);
    }

    public void Update()
    {
        loopList?.Update();
    }

    public override void Hide()
    {
        base.Hide();
        this.loopList?.OnResetList();
    }

    public override void Show()
    {
        base.Show();
        this.loopList.UpdatePanel();
    }
}
