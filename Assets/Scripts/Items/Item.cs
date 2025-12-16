using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Items
{
    [RequireComponent(typeof(ItemUI))]
    public class Item : MonoBehaviour
    {
        [SerializeField] private ItemData itemData; // Static info for every object of same type.
        [SerializeField] public bool isBought;      // Instance-specific info.

        public ItemData GetItemData()
        {
            return itemData;
        }
    }
}
