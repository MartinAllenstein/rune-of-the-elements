using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
   
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinCodeButton;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private TMP_InputField playerNameInputField;
    [SerializeField] private LobbyCreateUI lobbyCreateUI;

    private void Awake()
    {
        mainMenuButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.LeaveLobby();
            Loader.Load(Loader.Scene.MainMenuScene);
        });
        
        createLobbyButton.onClick.AddListener(() =>
        {
            lobbyCreateUI.Show();
        });
        
        quickJoinButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.QuickJoin();
        });
        
        joinCodeButton.onClick.AddListener(() =>
        {
            GameLobby.Instance.JoinWithCode(joinCodeInputField.text);
        });
    }

    private void Start()
    {
        playerNameInputField.text = KitchenGameMultiplayer.Instance.GetPlayerName();
        playerNameInputField.onValueChanged.AddListener((string newText) =>
        {
            KitchenGameMultiplayer.Instance.SetPlayerName(newText);
        });
    }
    
}
