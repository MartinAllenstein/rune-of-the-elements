using UnityEngine;
using System;
using Unity.Cinemachine;
using Unity.Netcode;

public class CinemachineTargetSetter : MonoBehaviour
{
    private CinemachineCamera virtualCamera;
    private bool hasTarget = false;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
    }

    private void Start()
    {
        Player.OnAnyPlayerSpawned += Player_OnAnyPlayerSpawned;
        
        TrySetTarget();
    }

    private void Player_OnAnyPlayerSpawned(object sender, EventArgs e)
    {
        TrySetTarget();
    }

    private void TrySetTarget()
    {
        if (!hasTarget && Player.LocalInstance != null)
        {
            virtualCamera.Follow = Player.LocalInstance.transform;
            //virtualCamera.LookAt = Player.LocalInstance.transform;

            hasTarget = true;

            Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
        }
    }

    private void OnDestroy()
    {
        Player.OnAnyPlayerSpawned -= Player_OnAnyPlayerSpawned;
    }
}