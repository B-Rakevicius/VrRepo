using UnityEngine;

namespace Items
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemData itemData; // Static info for every object of same type.
        public bool IsBought { get; set; }  // Instance-specific info.

        public ItemData GetItemData()
        {
            return itemData;
        }
    }
}
