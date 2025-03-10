using UnityEngine;

public class Utils 
{
    public static int RandomIntVale(int minValue,int maxValue)
    {
        return Random.Range(minValue,maxValue);
    }

    public static float RandomFloatVale(float minValue, float maxValue)
    {
        return Random.Range(minValue, maxValue);
    }

    public static Vector3 GetGemItemPos(int row , int col)
    {
        return new Vector3(GameCfg.offsetX * col, -GameCfg.offsetY * (row - 1), 0);
    }

    public static Vector3 GetNextPos(int row,int col)
    {
       return GetCurrentPos(row,col) + Vector3.down * GameCfg.offsetY;
    }

    public static Vector3 GetCurrentPos(int row, int col)
    {
        return GameCfg.startPoss[GameCfg.level - 1] + GetGemItemPos(row, col);
    }

    public static Vector3 GetStartPos(int row, int col)
    {
        return GameCfg.startPoss[GameCfg.level - 1] + new Vector3(GameCfg.offsetX * col, GameCfg.offsetY * 5, 0);
    }

    public static Vector3 GetNGUIPos(int row)
    {
        return Vector3.up * (GameCfg.uiYStartPos[GameCfg.level - 1] - row * GameCfg.uiYInterval);
    }

    public static Vector3 WorldTONGUIPos(Vector3 pos,Camera uiCamera)
    {
        // 步骤1: 世界坐标 → 屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(pos);
        // 步骤2: 屏幕坐标 → NGUI坐标
        Vector3 uiPos = uiCamera.ScreenToWorldPoint(screenPos);
        return uiCamera.transform.TransformPoint(uiPos);
    }

    // 计算二次贝塞尔曲线上的点
    public static Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        return uu * p0 + 2 * u * t * p1 + tt * p2;
    }

    public static int getGemsIdx(int percentNum)
    {
        for (int i = 0; i < GameCfg.gemsPercentages.Length; i++)
        {
            if(percentNum <= GameCfg.gemsPercentages[i])
            {
                return i;
            }
        }
        return 0;
    }

    // 四方向向量：上、下、左、右
    public static Vector2Int[] directions = {
        new Vector2Int(-1,0), //上
        new Vector2Int(1,0), //下
        new Vector2Int(0,-1), //左 
        new Vector2Int(0,1) //右
    };

    /// <summary>
    /// 转换XY到当前索引
    /// </summary>
    /// <param name="x">行索引</param>
    /// <param name="y">列索引</param>
    /// <returns></returns>
    public static int TransformXYTOIndex(int x, int y)
    {
       return (GameCfg.row - x - 1) * GameCfg.row + y;
    }
}