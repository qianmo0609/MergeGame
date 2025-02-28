using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [SerializeField] GameMgr gameManager;
    void Start()
    {
        EventCenter.Instance.OnInit();
        PoolManager.Instance.OnInit();
        ResManager.Instance.OnInit();
        LocalData.Instance.OnInit();
        CreateFactory.Instance.OnInit();
        Instantiate(gameManager);
    }

    private void OnDestroy()
    {
        gameManager.OnDestroy_();
        LocalData.Instance.OnDisable();
        PoolManager.Instance.OnDestroy();
        EventCenter.Instance.Disable();
    }
}
