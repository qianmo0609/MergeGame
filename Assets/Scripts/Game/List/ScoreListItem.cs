using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreListItem : MonoBehaviour
{
    [SerializeField] SpriteRenderer icon;
    //[SerializeField] SpriteRenderer symbol;
    [SerializeField] SpriteRenderer scoreShi;
    [SerializeField] SpriteRenderer scoreGe;

    public void OnSetInfo(Sprite icon,int num)
    {
        this.icon.sprite = icon;
        Vector3 pos = scoreGe.transform.localPosition;
        int shiValue = num / 10;
        if (shiValue > 0)
        {
            //如果是两位数
            scoreShi.gameObject.SetActive(true);
            pos.x = GameCfg.scoreListNumDoubleX;
            //TODO：设置数字
        }
        else
        {
            //如果是一位数
            scoreShi.gameObject.SetActive(false);
            pos.x = GameCfg.scoreListNumSingleX;
            //TODO：设置数字
        }
        scoreGe.transform.localPosition = pos;
    }

    public void OnHide()
    {

    }
}
