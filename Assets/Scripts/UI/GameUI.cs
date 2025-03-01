using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GameUI : MonoBehaviour
{
    [SerializeField] UIButton btnStart;
    [SerializeField] UIButton btnHandUp;
    [SerializeField] UIButton btnRecord;
    [SerializeField] UIButton btnInfo;

    private void Start()
    {
        btnStart.onClick.Add(new EventDelegate(OnStartEvent));
        btnHandUp.onClick.Add(new EventDelegate(OnHandUpEvent));
        btnRecord.onClick.Add(new EventDelegate(OnRecordEvent));
        btnInfo.onClick.Add(new EventDelegate(OnInfoEvent));
    }

    private void OnStartEvent()
    {
        //��������ڹһ�״̬��������ť,ִ�������߼�
        if (!GameCfg.isHandUp)
        {
            EventCenter.Instance.ExcuteEvent(EventNum.TestEvent);
        }
        else
        {
            //������ڹһ���״̬�������ڵľ���Ҫ����ʼ��ť����ͼ���ɿ�ʼ����ͼ
            //�һ���ť����ͼ���ɣ�δ�һ�����ͼ
            btnStart.normalSprite = "h5by_xyx_ks";
            btnHandUp.normalSprite = "h5by_xyx_gj";
            //ȡ���һ�
            GameCfg.isHandUp = false;
        }
        
    }
    private void OnHandUpEvent()
    {
        //��������ڹһ�״̬��������ť,ִ�������߼�
        if (!GameCfg.isHandUp)
        {
            GameCfg.isHandUp = true;
            btnStart.normalSprite = "h5by_xyx_qxgj";
            btnHandUp.normalSprite = "h5by_xyx_gjz";
            EventCenter.Instance.ExcuteEvent(EventNum.TestEvent);
        }
        //�һ�״̬�£���ȡ���һ�����
    }

    private void OnRecordEvent()
    {

    }

    private void OnInfoEvent()
    {

    }

    private void OnDestroy()
    {
        btnStart.onClick.Clear();
        btnHandUp.onClick.Clear();
        btnRecord.onClick.Clear();
        btnInfo.onClick.Clear();
    }
}

