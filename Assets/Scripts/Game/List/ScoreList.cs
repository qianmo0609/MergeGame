using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreList
{
    Transform ListObj;
    Vector3 currentPos;
    List<ScoreListItem> scoreListCollection;
    int maxNum;

    int currentButtomIdx;

    public ScoreList(Transform obj)
    {
        this.ListObj = obj;
        this.OnReset();
#if UNITY_EDITOR
        this.TestListDisplay();
#endif
    }

    public void OnReset()
    {
        maxNum = GameCfg.scoreListItemMaxNum;
        scoreListCollection = new List<ScoreListItem>(maxNum);
        currentPos = GameCfg.scoreListStartPoss[GameCfg.level - 1];
        currentButtomIdx = 0;
    }

    public void AddItem(MergeInfo mergeInfo)
    {
        //����һ���������壬������ɺ�����һ��Gem
        EffectFlyItem ef = CreateFactory.Instance.CreateGameObj<EffectFlyItem>(GameObjEunm.effectFlyItem);
        this.MoveItem();
        ef.OnInitInfo(Utils.GetNextPos(mergeInfo.row, mergeInfo.col),this.ListObj.TransformPoint(this.GetNextItemPos()), ResManager.Instance.gemsSprites[mergeInfo.type], this.GetCb());
    }

#if UNITY_EDITOR
    //�����б�������ʾ
    void TestListDisplay()
    {
        //new GameObject().AddComponent<Test>().StartCoroutine(Display());
    }

    IEnumerator Display()
    {
        while (Application.isPlaying)
        {
            this.DisplayGem();
            yield return new WaitForSeconds(1);
        }
        //ClearCollection();
    }
#endif

    public void DisplayGem()
    {
        //������ʾ��Item
        ScoreListItem sl = CreateFactory.Instance.CreateGameObj<ScoreListItem>(GameObjEunm.scoreListItem);
        sl.transform.parent = ListObj;
        sl.transform.localPosition = currentPos;
        scoreListCollection.Add(sl);
        currentPos += Vector3.up * GameCfg.scoreListItemInterval;
    }

    void MoveItem()
    {
        if (scoreListCollection.Count >= maxNum)
        {
            float y;
            #region ʹ��Sequence��ʽ
            Sequence sequence = DOTween.Sequence();
            ////����б����Ѿ�����6�������б�Item���ƣ� ��ײ���Item�ƶ����ϲ���ʾ
            //for (int i = 0; i < scoreListCollection.Count; i++)
            //{
            //    y = GameCfg.scoreListStartPoss[GameCfg.level - 1].y + (i - 1) * GameCfg.scoreListItemInterval;
            //    sequence.Join(scoreListCollection[i].transform.DOLocalMoveY(y, 1.2f));
            //}
            for (int i = 0; i < scoreListCollection.Count; i++)
            {
                y = currentPos.y - GameCfg.scoreListItemInterval * (GameCfg.scoreListItemMaxNum - this.MappintIdx(i) + 1);
                sequence.Join(scoreListCollection[i].transform.DOLocalMoveY(y, .3f));
            }
            sequence.Play();//.OnComplete(this.MoveButtomItem);
            #endregion
            //for (int i = 0; i < scoreListCollection.Count; i++)
            //{
            //    y = currentPos.y - GameCfg.scoreListItemInterval * (GameCfg.scoreListItemMaxNum - i + 1);
            //    scoreListCollection[i].transform.DOLocalMoveY(y, 1.2f);
            //}
        }
    }

    Action GetCb()
    {
         return scoreListCollection.Count < maxNum?this.DisplayGem:MoveButtomItem;
    }

    public void MoveButtomItem()
    {
        scoreListCollection[currentButtomIdx].transform.localPosition = this.GetCurrentItemPos();
        this.currentButtomIdx++;
        this.currentButtomIdx %= GameCfg.scoreListItemMaxNum;
    }

    public int MappintIdx(int i)
    {
        int idx = i - this.currentButtomIdx;
        if (idx < 0)
        {
            idx += GameCfg.scoreListItemMaxNum;
            
        }
        return idx;
    }

    public Vector3 GetCurrentItemPos()
    {
        return scoreListCollection.Count < maxNum ? currentPos:currentPos - Vector3.up * GameCfg.scoreListItemInterval;
    }

    public Vector3 GetNextItemPos()
    {
        return scoreListCollection.Count < maxNum ? currentPos + new Vector3(-0.2686f, GameCfg.scoreListItemInterval, 0) : currentPos + new Vector3(-0.2686f, 0, 0);
    }

    public void ClearCollection()
    {
        ScoreListItem sl;
        for (int i = 0; i < scoreListCollection.Count; i++)
        {
            sl = scoreListCollection[i];
            Vector3 tarPos = sl.transform.position + new Vector3(Utils.RandomFloatVale(-1.0f, 1f), Utils.RandomFloatVale(0, 1.0f), 0);
            sl.transform.DOMove(tarPos, 0.2f).SetEase(Ease.OutSine).OnComplete(() =>
            {
                //��ʯ����
                sl.transform.DOMoveY(-10, Utils.RandomFloatVale(0.1f, 0.3f)).SetEase(Ease.InExpo);
            });
        }
    }
}
