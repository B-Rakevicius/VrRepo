using System.Collections.Generic;
using System.Threading.Tasks;
using Items;
using TMPro;
using UnityEngine;

namespace Shop
{
    public class ShoppingCart : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI checkoutDisplayText;

        private List<Item> cartItems;
        private int _totalPrice;

        private void Start()
        {
            cartItems = new List<Item>();
            
            // Reset the text to default
            checkoutDisplayText.fontSize = 0.05f;
            checkoutDisplayText.color = Color.red;
            checkoutDisplayText.text = "No Items";
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Is object an interactable item?
            Item item = other.GetComponentInParent<Item>();
            if (item != null)
            {
                ItemData itemData = item.GetItemData();
                
                // Item hasn't been bought before. Allow to buy.
                if (!item.IsBought)
                {
                    cartItems.Add(item);
                    _totalPrice += itemData.itemPrice;
                    
                    // Update checkout display text
                    checkoutDisplayText.fontSize = 0.08f;
                    checkoutDisplayText.color = Color.green;
                    checkoutDisplayText.text = "$" + _totalPrice;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Is object an interactable item?
            Item item = other.GetComponentInParent<Item>();
            if (item != null)
            {
                ItemData itemData = item.GetItemData();

                if (!item.IsBought)
                {
                    cartItems.Remove(item);
                    _totalPrice -= itemData.itemPrice;
                    
                    // Update checkout display text
                    checkoutDisplayText.fontSize = 0.08f;
                    checkoutDisplayText.color = Color.green;
                    checkoutDisplayText.text = "$" + _totalPrice;

                    if (_totalPrice <= 0)
                    {
                        checkoutDisplayText.fontSize = 0.05f;
                        checkoutDisplayText.color = Color.red;
                        checkoutDisplayText.text = "No Items";
                    }
                }
            }
        }

        public async void TryPurchaseItems()
        {
            if (cartItems.Count == 0) return;
            
            if (ShopManager.Instance.TryPurchase(cartItems))
            {
                cartItems.Clear();
                // TODO: Put these lines in a method for reusability
                checkoutDisplayText.fontSize = 0.08f;
                checkoutDisplayText.color = Color.green;
                checkoutDisplayText.text = "$$$";
                
                await Task.Delay(2000);
                
                checkoutDisplayText.fontSize = 0.04f;
                checkoutDisplayText.color = Color.green;
                checkoutDisplayText.text = "Take Items";
            }
        }

        public void ClearShoppingCart()
        {
            cartItems.Clear();
        }
    }
}
