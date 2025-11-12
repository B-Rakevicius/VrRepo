using System.Collections.Generic;
using System.Linq;
using Items;
using UI;
using UnityEngine;

namespace Shop
{
    public class ShopItemSpawner : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;
        
        
        private void Start()
        {
            ShopManager.Instance.OnItemPoolReceived += ShopManager_ItemPoolReceived;
        }

        private void OnDestroy()
        {
            ShopManager.Instance.OnItemPoolReceived -= ShopManager_ItemPoolReceived;
        }

        private void ShopManager_ItemPoolReceived(object sender, ShopManager.OnItemPoolReceivedEventArgs e)
        {
            Debug.Log("Item pool received.");
            ItemData itemData = FilterItems(e.items);
            SpawnItem(itemData.itemPrefab);
        }

        private ItemData FilterItems(List<ItemData> items)
        {
            float weight = items.Sum(x => x.spawnChance);
            float roll = Random.Range(0f, weight);

            items.Sort((a,b) => a.spawnChance.CompareTo(b.spawnChance));
            float cumulative = 0f;
            foreach (ItemData item in items)
            {
                cumulative += item.spawnChance;
                if (roll < cumulative)
                {
                    return item;
                }
            }

            // No item found. Something wrong must've happened.
            return null;
        }

        private void SpawnItem(GameObject itemPrefab)
        {
            if (itemPrefab != null)
            {
                Debug.Log("Spawning item...");
                GameObject shopItem = Instantiate(itemPrefab, spawnPoint.position, spawnPoint.rotation);
                ItemUI itemUI = shopItem.GetComponent<ItemUI>();
                itemUI.ToggleUI();
            }
            else
            {
                Debug.LogError("ShopItemSpawner: Item not valid!");
            }
        }
    }
}
