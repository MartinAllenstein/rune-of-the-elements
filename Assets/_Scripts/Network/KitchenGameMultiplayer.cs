using Unity.Netcode;
using UnityEngine;

public class KichenGameMultiplayer : NetworkBehaviour
{
    
    public static KichenGameMultiplayer Instance { get; private set; }
    
    [SerializeField] private KitchenObjectListSO kitchenObjectListSO;

    private void Awake()
    {
        Instance = this;
    }
    
    public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent)
    {
        SpawnKitchenObjectsServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnKitchenObjectsServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
    {
        KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
        
        Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.prefab);
                
                NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
                kitchenObjectNetworkObject.Spawn(true);
                
                KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
                    
                kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
                IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
                
                kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
    }

    private int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
    {
        return kitchenObjectListSO.kitchenObjectSOList.IndexOf(kitchenObjectSO);
    }

    private KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
    {
        return kitchenObjectListSO.kitchenObjectSOList[kitchenObjectSOIndex];
    }
}
