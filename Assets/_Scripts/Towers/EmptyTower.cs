using UnityEngine;

public class EmptyTower : BaseCounter
{
    [SerializeField] private Tower_ElementSO elementToTower;

    public override void Interact(Player player)
    {
        if (player.HasKitchenObject() && !HasKitchenObject())
        {
            KitchenObjectSO elementSO = player.GetKitchenObject().GetKitchenObjectSO();

            // Check if element can build Tower
            if (elementToTower.TryGetTowerData(elementSO, out TowerDataSO towerToBuild))
            {
                player.GetKitchenObject().DestroySelf();

                // build new Tower
                Instantiate(towerToBuild.towerPrefab, transform.position, transform.rotation);

                // destroy EmptyTower
                Destroy(gameObject);
            }
        }
    }
}