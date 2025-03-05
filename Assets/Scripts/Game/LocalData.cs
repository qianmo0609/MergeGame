using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ScoreData
{
    public int num;
    public int type;
}

public class LocalData : Singleton<LocalData>
{
    List<ScoreData> scoreDatas;

    public List<ScoreData> ScoreDatas { get => scoreDatas;}

    public override void OnInit()
    {
        //scoreDatas = new List<ScoreData>();
        scoreDatas = new List<ScoreData>
        {
            new ScoreData{type = 0 , num = 3},
            new ScoreData{type = 3 , num = 1},
            new ScoreData{type = 5, num = 2},
            new ScoreData{type = 2 , num = 1},
            new ScoreData{type = 1 , num = 5},
            new ScoreData{type = 8 , num = 3},
            new ScoreData{type = 8 , num = 1},
            new ScoreData{type = 8 , num = 2},
            new ScoreData{type = 8 , num = 3},
            new ScoreData{type = 8 , num = 4},
        };
        base.OnInit();
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
