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
        // ��ʼ���б���
        InitializeList();
        // ����ɼ��б����������б����С
        CalculateVisibleItems();
        // ���������¼�
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
            // �����б�������ݣ�����������ı�
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
        // �����б���Ĵ�С
        itemSize = grid.cellHeight;
        // ����ɼ��б��������
        visibleItemCount = Mathf.FloorToInt(scrollView.panel.height / itemSize);
    }

    void OnScrollFinished()
    {
        // ��ȡ������ͼ��λ��
        Vector3 scrollPosition = scrollView.transform.localPosition;
        // �жϹ������򲢴���ѭ���߼�
        if (scrollPosition.y > 0)
        {
            // ���Ϲ���
            while (scrollPosition.y > itemSize)
            {
                MoveLastItemToTop();
                scrollPosition.y -= itemSize;
                scrollView.transform.localPosition = scrollPosition;
            }
        }
        else if (scrollPosition.y < 0)
        {
            // ���¹���
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
