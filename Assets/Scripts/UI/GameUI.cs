using System.Text;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    [SerializeField] UIButton btnStart;
    [SerializeField] UIButton btnHandUp;
    [SerializeField] UIButton btnRecord;
    [SerializeField] UIButton btnInfo;
    [SerializeField] UILabel txtTotalScore;
    StringBuilder totalScore;

    private void Start()
    {
        btnStart.onClick.Add(new EventDelegate(OnStartEvent));
        btnHandUp.onClick.Add(new EventDelegate(OnHandUpEvent));
        btnRecord.onClick.Add(new EventDelegate(OnRecordEvent));
        btnInfo.onClick.Add(new EventDelegate(OnInfoEvent));
        totalScore = new StringBuilder(10);
        EventCenter.Instance.RegisterEvent(EventNum.UpdateTotalScoreEvent,this.UpdateTotalScore);
    }

    private void OnStartEvent()
    {
        //如果不是在挂机状态，则点击按钮,执行正常逻辑
        if (!GameCfg.isHandUp)
        {
            EventCenter.Instance.ExcuteEvent(EventNum.TestEvent);
        }
        else
        {
            //如果是在挂机的状态，则现在的就需要将开始按钮的贴图换成开始的贴图
            //挂机按钮的贴图换成，未挂机的贴图
            btnStart.normalSprite = "h5by_xyx_ks";
            btnHandUp.normalSprite = "h5by_xyx_gj";
            //取消挂机
            GameCfg.isHandUp = false;
        }
        
    }
    private void OnHandUpEvent()
    {
        //如果不是在挂机状态，则点击按钮,执行正常逻辑
        if (!GameCfg.isHandUp)
        {
            GameCfg.isHandUp = true;
            btnStart.normalSprite = "h5by_xyx_qxgj";
            btnHandUp.normalSprite = "h5by_xyx_gjz";
            EventCenter.Instance.ExcuteEvent(EventNum.TestEvent);
        }
        //挂机状态下，有取消挂机控制
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
        totalScore.Append("x");
        totalScore.Append(GameCfg.totalScore.ToString());
        this.txtTotalScore.text = totalScore.ToString();
    }

    private void OnDestroy()
    {
        btnStart.onClick.Clear();
        btnHandUp.onClick.Clear();
        btnRecord.onClick.Clear();
        btnInfo.onClick.Clear();
    }
}

