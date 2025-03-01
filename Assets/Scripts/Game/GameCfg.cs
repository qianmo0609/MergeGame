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

    public static GameState gameState = GameState.idle; //是否正在匹配

    public static Vector3 buttomWallStartPos = new Vector3(-1.85f,-2.378f,0);
    public static int buttomWallNum = 16;

    public static Vector3[] wall = new Vector3[] { new Vector3(-1.669f,-2.057f,0), new Vector3(1.709f, -2.057f,0) };

    public static float uiYInterval = 75f;

    public static float[] uiYStartPos = new float[] { 0, 0, 0 };

    public static float scoreListItemInterval = .5f;

    public static int scoreListItemMaxNum = 6;

    public static Vector3[] scoreListStartPoss = new Vector3[] { new Vector3(0, -0.154f,0), new Vector3(0, 0, 0) , new Vector3(0, 0, 0) };

    public static float scoreListNumDoubleX = 0.565f;

    public static float scoreListNumSingleX = 0.488f;

    public static int wallNum = 15;

    public static float flyTOPosOffsetX = -0.2686f;

    public static float flyBezierOffsetY = 3f;

    public static Vector4 spriteClipRange = new Vector4(-5,-3,-1.8f,1.15f);
    public static Vector4 spriteRange = new Vector4(-10,10,-10,10);
}

public enum GameState
{
    idle,
    isMatching,
    gameOver,
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
    scoreListItem,
    bombEffct
}
