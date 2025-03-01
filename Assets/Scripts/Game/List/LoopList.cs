using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LoopList:UIBase
{
    public UIScrollView scrollView;
    public UIGrid grid;
    public GameObject listItemPrefab;
    public int itemCount = 20;

    private List<GameObject> listItems = new List<GameObject>();
    private int visibleItemCount;
    private float itemSize;

    void Start()
    {
        // 初始化列表项
        InitializeList();
        // 计算可见列表项数量和列表项大小
        CalculateVisibleItems();
        // 监听滚动事件
        scrollView.onDragFinished = OnScrollFinished;
    }

    void InitializeList()
    {
        for (int i = 0; i < itemCount; i++)
        {
            GameObject item = Instantiate(listItemPrefab) as GameObject;
            item.transform.parent = grid.transform;
            item.transform.localScale = Vector3.one;
            listItems.Add(item);
            // 设置列表项的内容，这里简单设置文本
            UILabel label = item.GetComponentInChildren<UILabel>();
            if (label != null)
            {
                label.text = "Item " + i;
            }
        }
        grid.Reposition();
    }

    void CalculateVisibleItems()
    {
        // 计算列表项的大小
        itemSize = grid.cellHeight;
        // 计算可见列表项的数量
        visibleItemCount = Mathf.FloorToInt(scrollView.panel.height / itemSize);
    }

    void OnScrollFinished()
    {
        // 获取滚动视图的位置
        Vector3 scrollPosition = scrollView.transform.localPosition;
        // 判断滚动方向并处理循环逻辑
        if (scrollPosition.y > 0)
        {
            // 向上滚动
            while (scrollPosition.y > itemSize)
            {
                MoveLastItemToTop();
                scrollPosition.y -= itemSize;
                scrollView.transform.localPosition = scrollPosition;
            }
        }
        else if (scrollPosition.y < 0)
        {
            // 向下滚动
            while (scrollPosition.y < -itemSize)
            {
                MoveFirstItemToBottom();
                scrollPosition.y += itemSize;
                scrollView.transform.localPosition = scrollPosition;
            }
        }
    }

    void MoveLastItemToTop()
    {
        GameObject lastItem = listItems[listItems.Count - 1];
        listItems.RemoveAt(listItems.Count - 1);
        listItems.Insert(0, lastItem);
        lastItem.transform.SetAsFirstSibling();
        grid.Reposition();
    }

    void MoveFirstItemToBottom()
    {
        GameObject firstItem = listItems[0];
        listItems.RemoveAt(0);
        listItems.Add(firstItem);
        firstItem.transform.SetAsLastSibling();
        grid.Reposition();
    }
}
