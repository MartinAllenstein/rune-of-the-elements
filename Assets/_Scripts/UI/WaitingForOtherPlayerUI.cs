using System;
using UnityEngine;

public class WaitingForOtherPlayerUI : MonoBehaviour
{
    private void Start()
    {
        GameManager.Instance.OnLocalPlayerReadyChange += GameManager_OnLocalPlayerReadyChange;
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        
        Hide();
    }

    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            Hide();
        }
    }

    private void GameManager_OnLocalPlayerReadyChange(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsLocalPlayerReady())
        {
            Show();
        }
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
