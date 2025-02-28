using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCfg
{
    public static int row = 4;
    public static int col = 4;

    public static float offsetX = 0.68f;
    public static float offsetY = 0.64f;

    public static int[] gridPerRow = new int[3] { 4, 5, 6 };

    public static int level = 1;

    public static bool isHandUp = false; //是否挂起

    public static Vector3[] startPoss = new Vector3[] { new Vector3(-1f, 0.03f, 0), new Vector3(-1f, 0.03f, 0), new Vector3(-1f, 0.03f, 0) };

    public static bool isMath = false; //是否正在匹配

    public static Vector3 buttomWallStartPos = new Vector3(-1.85f,-2.378f,0);

    public static Vector3[] wall = new Vector3[] { new Vector3(-1.669f,-2.057f,0), new Vector3(1.709f, -2.057f,0) };

    public static float uiYInterval = 5f;

    public static float[] uiYStartPos = new float[] { 0, 0, 0 };

    public static float scoreListItemInterval = .5f;

    public static int scoreListItemMaxNum = 6;

    public static Vector3[] scoreListStartPoss = new Vector3[] { new Vector3(0,-0.154f,0), new Vector3(0, 0, 0) , new Vector3(0, 0, 0) };

    public static float scoreListNumDoubleX = 0.565f;

    public static float scoreListNumSingleX = 0.488f;
}

public enum GameObjEunm
{
    None,
    gemItem,
    bomb,
    slot,
    effectItem,
    effectTextItem,
    effectFlyItem,
    loopListItem,
    scoreListItem
}
