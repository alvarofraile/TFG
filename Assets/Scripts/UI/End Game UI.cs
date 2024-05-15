using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EndGameUI : MonoBehaviour
{
    [SerializeField] private GameObject background;
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        GameManager.Instance.OnGameEnd += GameManager_OnGameEnd;
    }

    private void GameManager_OnGameEnd(object sender, GameManager.OnGameEndEventArgs e)
    {
        background.SetActive(true);
        text.gameObject.SetActive(true);
        text.text = e.gameResult.ToString();
    }

}
