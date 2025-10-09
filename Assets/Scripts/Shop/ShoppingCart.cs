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

        private List<Item> cartItemsBought; // Used to check if the cart has been emptied after purchase.

        private void Start()
        {
            cartItems = new List<Item>();
            cartItemsBought = new List<Item>();
            
            // Reset the text to default
            checkoutDisplayText.fontSize = 0.05f;
            checkoutDisplayText.color = Color.red;
            checkoutDisplayText.text = "No Items";
        }
        
        private void OnTriggerEnter(Collider other)
        {
            // Is object an interactable item?
            Item item = other.GetComponentInParent<Item>();
            if (item != null && cartItemsBought.Count == 0)
            {
                // Item hasn't been bought before. Allow to buy.
                if (!item.IsBought)
                {
                    ItemData itemData = item.GetItemData();
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
                // Item was purchased but not removed. Remove it.
                if (cartItemsBought.Contains(item))
                {
                    cartItemsBought.Remove(item);

                    if (cartItemsBought.Count == 0)
                    {
                        checkoutDisplayText.fontSize = 0.05f;
                        checkoutDisplayText.color = Color.red;
                        checkoutDisplayText.text = "No Items";
                    }
                    
                    return;
                }
                
                if (!item.IsBought && cartItems.Contains(item))
                {
                    ItemData itemData = item.GetItemData();
                    cartItems.Remove(item);
                    _totalPrice -= itemData.itemPrice;
                    
                    // Update checkout display text
                    checkoutDisplayText.fontSize = 0.08f;
                    checkoutDisplayText.color = Color.green;
                    checkoutDisplayText.text = "$" + _totalPrice;

                    if (cartItems.Count == 0)
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
                // Add cart items to a new list (to keep track when all items had been taken from cart after purchase)
                foreach (Item item in cartItems)
                {
                    cartItemsBought.Add(item);
                }
                cartItems.Clear();

                _totalPrice = 0;
                // TODO: Put these lines in a method for reusability
                checkoutDisplayText.fontSize = 0.08f;
                checkoutDisplayText.color = Color.green;
                checkoutDisplayText.text = "$$$";
                
                await Task.Delay(2000);

                if (cartItemsBought.Count != 0)
                {
                    checkoutDisplayText.fontSize = 0.04f;
                    checkoutDisplayText.color = Color.green;
                    checkoutDisplayText.text = "Take Items";
                }
            }
        }
    }
}
