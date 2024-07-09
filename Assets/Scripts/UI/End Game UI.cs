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
        StartCoroutine(ShowResult(e.gameResult.ToString(), 1.5f));
    }

    IEnumerator ShowResult(string message, float delay) {
        background.SetActive(true);
        text.gameObject.SetActive(true);
        text.text = message;

        yield return new WaitForSeconds(delay);

        background.SetActive(false);
        text.gameObject.SetActive(false);
    }

}
