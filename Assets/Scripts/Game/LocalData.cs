using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreData
{
    public int score;
    public int type;
}

public class LocalData : Singleton<LocalData>
{
    List<ScoreData> scoreDatas;

    public override void OnInit()
    {
        base.OnInit();
        scoreDatas = new List<ScoreData>();
    }

    public void AddScoreData(ScoreData scoreData)
    {
        scoreDatas.Add(scoreData);
    }

    public List<ScoreData> GetScoreData()
    {
        return scoreDatas;
    }

    public void OnDisable()
    {
        scoreDatas.Clear();
        scoreDatas = null;
    }
}
