using Unity.Netcode;
using UnityEngine;

public class EmptyTower : BaseCounter
{
    [SerializeField] private Tower_ElementSO elementToTower;

    public override void Interact(Player player)
    {
        InteractServerRpc(player.OwnerClientId);
    }
    
    
    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ulong clientId)
    {
        Player player = KitchenGameMultiplayer.Instance.GetPlayerFromClientId(clientId);

        if (player == null) return;

        if (player.HasKitchenObject())
        {
            KitchenObjectSO elementSO = player.GetKitchenObject().GetKitchenObjectSO();

            // Check if element can build Tower
            if (elementToTower.TryGetTowerData(elementSO, out TowerDataSO towerToBuild))
            {
                KitchenObject.DestroyKitchenObject(player.GetKitchenObject());

                // build new Tower
                GameObject towerGameObject = Instantiate(towerToBuild.towerPrefab, transform.position, transform.rotation);
                NetworkObject towerNetworkObject = towerGameObject.GetComponent<NetworkObject>();
                towerNetworkObject.Spawn(true);
                
                // destroy EmptyTower
                GetComponent<NetworkObject>().Despawn(true); 
            }
        }
    }
}