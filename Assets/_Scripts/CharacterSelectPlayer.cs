using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private PlayerVisual playerVisual;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshPro playerNameText;

    private void Awake()
    {
        kickButton.onClick.AddListener(() =>
        {
            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            GameLobby.Instance.KickPlayer(playerData.playerId.ToString());
            KitchenGameMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }

    private void Start()
    {
        KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;
        
        kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
    {
        UpdatePlayer();
    }

    private void UpdatePlayer()
    {
        if (KitchenGameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();

            PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            
            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));

            playerVisual.SetPlayerVisual(KitchenGameMultiplayer.Instance.GetPlayerVisual(playerData.visualId));
            
            playerNameText.text = playerData.playerName.ToString();
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
    
    private void OnDestroy()
    {
        if (KitchenGameMultiplayer.Instance != null)
        {
            KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
        }
        
        if (CharacterSelectReady.Instance != null)
        {
            CharacterSelectReady.Instance.OnReadyChanged -= CharacterSelectReady_OnReadyChanged;
        }
    }
}
