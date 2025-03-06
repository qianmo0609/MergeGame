using System.Collections;
using System.Text;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] UIButton btnStart;
    [SerializeField] UIButton btnHandUp;
    [SerializeField] UIButton btnRecord;
    [SerializeField] UIButton btnInfo;
    [SerializeField] UILabel txtTotalScore;
    [SerializeField] UILabel txtCombo;
    [SerializeField] TweenAlpha tweenAlpha;
    StringBuilder totalScore;
    StringBuilder comboSB;

    Coroutine closeComboTxtCoroutine;

    private void Start()
    {
        btnStart.onClick.Add(new EventDelegate(OnStartEvent));
        btnHandUp.onClick.Add(new EventDelegate(OnHandUpEvent));
        btnRecord.onClick.Add(new EventDelegate(OnRecordEvent));
        btnInfo.onClick.Add(new EventDelegate(OnInfoEvent));
        totalScore = new StringBuilder(10);
        comboSB = new StringBuilder(2);
        EventCenter.Instance.RegisterEvent(EventNum.UpdateTotalScoreEvent, this.UpdateTotalScore);
        EventCenter.Instance.RegisterEvent(EventNum.ComboDisplayNumEvent, this.UpdateComboTxt);
        EventCenter.Instance.RegisterEvent(EventNum.EnableOrDisableBtnStartEvent, this.EnableOrDisableStartBtn);
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
            btnStart.normalSprite = ConstValue.btnStartNormalSpriteName; //"h5by_xyx_ks";
            btnHandUp.normalSprite = ConstValue.btnHandUpNormalSpriteName;//"h5by_xyx_gj";
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
            btnStart.normalSprite = ConstValue.btnStartHandUpSpriteName;//"h5by_xyx_qxgj";
            btnHandUp.normalSprite = ConstValue.btnHandUpHandUpSpriteName;//"h5by_xyx_gjz";
            EventCenter.Instance.ExcuteEvent(EventNum.TestEvent);
        }
        //�һ�״̬�£���ȡ���һ�����
    }

    private void OnRecordEvent()
    {
        UIManager.Instance.GetWindow<RecordUI>().Show();
    }

    private void OnInfoEvent()
    {
        UIManager.Instance.GetWindow<RuleUI>().Show();
    }

    private void UpdateTotalScore()
    {
        totalScore.Clear();
        totalScore.Append(ConstValue.symbolX);
        totalScore.Append(GameCfg.totalScore.ToString());
        this.txtTotalScore.text = totalScore.ToString();
    }

    private void UpdateComboTxt()
    {
        if (GameCfg.comboNum <= 0) return;
        //this.txtCombo.gameObject.SetActive(true);
        tweenAlpha.PlayReverse();
        comboSB.Clear();
        comboSB.Append(GameCfg.comboNum);
        this.txtCombo.text = comboSB.ToString();
        GameCfg.comboNum = 0;
        closeComboTxtCoroutine = StartCoroutine(CloseComboTxt());
    }

    IEnumerator CloseComboTxt()
    {
        yield return new WaitForSeconds(1);
        tweenAlpha.PlayForward();
        //this.txtCombo.gameObject.SetActive(false);
        if (closeComboTxtCoroutine != null)
        {
            StopCoroutine(closeComboTxtCoroutine);
        }
    }

    void EnableOrDisableStartBtn()
    {
        if (GameCfg.isHandUp) return;
        this.btnStart.enabled = GameCfg.isEnableBtnStart;
        this.btnStart.normalSprite = GameCfg.isEnableBtnStart ? ConstValue.btnStartNormalSpriteName : ConstValue.btnStartDisableSpriteName;
    }

    private void OnDestroy()
    {
        comboSB.Clear();
        comboSB = null;
        totalScore.Clear();
        totalScore = null;
        closeComboTxtCoroutine = null;
        btnStart.onClick.Clear();
        btnHandUp.onClick.Clear();
        btnRecord.onClick.Clear();
        btnInfo.onClick.Clear();
        EventCenter.Instance.UnregisterEvent(EventNum.UpdateTotalScoreEvent);
        EventCenter.Instance.UnregisterEvent(EventNum.ComboDisplayNumEvent);
        EventCenter.Instance.UnregisterEvent(EventNum.EnableOrDisableBtnStartEvent);
    }
}

