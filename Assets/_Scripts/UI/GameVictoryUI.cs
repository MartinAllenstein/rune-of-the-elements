using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameVictoryUI : MonoBehaviour
{
    
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button nextLevelButton;
    
    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer) 
        {
            mainMenuButton.gameObject.SetActive(false);
            nextLevelButton.gameObject.SetActive(false);
        }
        
        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        
        nextLevelButton.onClick.AddListener(() =>
        {
            Loader.LoadNextLevel();
        });
    }
    
    
    private void Start()
    {
        GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
        
        Hide();
    }

    
    private void GameManager_OnStateChanged(object sender, EventArgs e)
    {
        if (GameManager.Instance.IsVictory())
        {
            Show();
        }
        else
        {
            Hide();
        }
    }


    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
