using UnityEngine;

namespace Items
{
    [CreateAssetMenu(menuName = "Items/Shop Item")]
    public class ItemData : ScriptableObject
    {
        public string itemName;
        public string itemDescription;
        public int itemPrice;
        public int unlocksAt;  // From which round item is available to buy.
        public float spawnChance;
        public GameObject itemPrefab;
    }
}
