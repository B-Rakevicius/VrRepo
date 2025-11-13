using Items;
using TMPro;
using Utils;
using UnityEngine;

namespace UI
{
    public class ItemUI : MonoBehaviour
    {
        [Tooltip("Reference to the item data component. If not provided, it will be found automatically")]
        [SerializeField] 
        private Item _item;

        private ItemData _itemData;

        [Tooltip("What item info panel to show when the item has already been bought?")]
        [SerializeField] private RectTransform _boughtItemInfo;
        [SerializeField] private TextMeshProUGUI _itemNameBought;
        [SerializeField] private TextMeshProUGUI _itemDescriptionBought;
        
        [Tooltip("What item info panel to show when the item hasn't been bought?")]
        [SerializeField] private RectTransform _shopItemInfo;
        [SerializeField] private TextMeshProUGUI _itemNameShop;
        [SerializeField] private TextMeshProUGUI _itemDescriptionShop;
        [SerializeField] private TextMeshProUGUI _itemPriceShop;
        
        [Header("Animation Settings")] 
        [SerializeField] private float animDuration = 1f;
        [SerializeField] private float animStartScale = 0f;
        [SerializeField] private float animEndScale = 1f;

        public float AnimDuration => animDuration;
        private bool m_isUIOpen = false;

        private void Start()
        {
            // Get reference to ItemData (to know later if the item is bought or not)
            if (_item == null)
            {
                _item = GetComponent<Item>();
            }
            _itemData = _item.GetItemData();
            
            string itemName = _itemData.itemName;
            string itemDescription = _itemData.itemDescription;
            int itemPrice = _itemData.itemPrice;

            if (_shopItemInfo != null)
            {
                _itemNameShop.text = itemName;
                _itemDescriptionShop.text = itemDescription;
                _itemPriceShop.text = "$" + itemPrice;
                Hide(_shopItemInfo);
            }

            if (_boughtItemInfo != null)
            {
                _itemNameBought.text = itemName;
                _itemDescriptionBought.text = itemDescription;
                Hide(_boughtItemInfo);
            }
        }

        /// <summary>
        /// Toggles item's UI depending on whether the item is already bought or not. Mainly used from HandInteractableChecker
        /// </summary>
        public async void ToggleUI()
        {
            bool isBought = _item.IsBought;

            if (isBought) // Activate UI panel which contains item info without its price
            {
                if (_boughtItemInfo == null) { return; }
                
                if (!m_isUIOpen)
                {
                    Show(_boughtItemInfo);
                    await UIAnimator.ScaleAnim(_boughtItemInfo, animDuration, animStartScale, animEndScale);
                }
                else
                {
                    await UIAnimator.ScaleAnim(_boughtItemInfo, animDuration, animEndScale, animStartScale);
                    Hide(_boughtItemInfo);
                }
                m_isUIOpen = !m_isUIOpen;
            }
            else // Item is not bought, display full panel
            {
                if(_shopItemInfo == null) { return; }
                
                if (!m_isUIOpen)
                {
                    Show(_shopItemInfo);
                    await UIAnimator.ScaleAnim(_shopItemInfo, animDuration, animStartScale, animEndScale);
                }
                else
                {
                    await UIAnimator.ScaleAnim(_shopItemInfo, animDuration, animEndScale, animStartScale);
                    Hide(_shopItemInfo);
                }
                m_isUIOpen = !m_isUIOpen;
            }
        }

        /// <summary>
        /// Shows item UI when hovering on the item.
        /// </summary>
        public async void ShowUI()
        {
            if(m_isUIOpen) { return; }
            
            bool isBought = _item.IsBought;

            if (!isBought) // Activate UI panel which contains item info without its price
            {
                Show(_boughtItemInfo);
                await UIAnimator.ScaleAnim(_boughtItemInfo, animDuration, animStartScale, animEndScale);
                m_isUIOpen = true;
            }
        }

        /// <summary>
        /// Hides item UI when not hovering.
        /// </summary>
        public async void HideUI()
        {
            if(!m_isUIOpen) { return; }
            
            bool isBought = _item.IsBought;

            if (!isBought) // Activate UI panel which contains item info without its price
            {
                await UIAnimator.ScaleAnim(_boughtItemInfo, animDuration, animEndScale, animStartScale);
                Hide(_boughtItemInfo);
                m_isUIOpen = false;
            }
        }

        private void Hide(RectTransform target)
        {
            target.gameObject.SetActive(false);
        }
        
        private void Show(RectTransform target)
        {
            target.gameObject.SetActive(true);
        }
    }
}
