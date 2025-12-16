using System.Collections.Generic;
using System.Threading.Tasks;
using Items;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Shop
{
    public class ShoppingCart : MonoBehaviour
    {
        [Tooltip("Displays shopping cart's total price")]
        [SerializeField] private TextMeshProUGUI checkoutDisplayText;
        [Tooltip("Displays player's current money")]
        [SerializeField] private TextMeshProUGUI moneyText;
        [Tooltip("Audio Source from radio object")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Audio clip to play")]
        [SerializeField] private AudioClip audioClip;

        //private List<Item> cartItems;
        private HashSet<Item> cartItems;
        private int _totalPrice;

        private List<Item> cartItemsBought; // Used to check if the cart has been emptied after purchase.

        private void Start()
        {
            // Subscribe to OnMoneyChanged event so that "CurrentBalance" text could be updated
            PlayerManager.Instance.OnMoneyChanged += PlayerManager_OnMoneyChanged;

            // Play shop music as soon as it spawns
            if (audioSource == null)
            {
                audioSource = GetComponentInParent<AudioSource>();
            }
            audioSource.clip = audioClip;
            audioSource.loop = true;
            audioSource.spatialize = true;
            audioSource.spatialBlend = 1f;
            audioSource.Play();
            
            cartItems = new HashSet<Item>();
            cartItemsBought = new List<Item>();
            
            // Reset the text to default
            checkoutDisplayText.fontSize = 0.05f;
            checkoutDisplayText.color = Color.red;
            checkoutDisplayText.text = "No Items";
            
            // Display current money on start
            moneyText.text = $"${PlayerManager.Instance.CurrentMoney}";
            
        }

        private void OnDestroy()
        {
            PlayerManager.Instance.OnMoneyChanged -= PlayerManager_OnMoneyChanged;
        }

        private void PlayerManager_OnMoneyChanged(object sender, PlayerManager.OnMoneyChangedEventArgs e)
        {
            moneyText.text = "$" + e.Money;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Is object an interactable item?
            Item item = other.GetComponentInParent<Item>();
            
            // Check if bought items have been removed from the cart
            if (item != null && cartItemsBought.Count == 0)
            {
                // Item hasn't been bought before and is inserted in the cart before. Allow to buy.
                if (!item.isBought)
                {
                    ItemData itemData = item.GetItemData();
                    if (cartItems.Add(item))
                    {
                        _totalPrice += itemData.itemPrice;
                        
                        // Update checkout display text
                        checkoutDisplayText.fontSize = 0.08f;
                        checkoutDisplayText.color = Color.green;
                        checkoutDisplayText.text = "$" + _totalPrice;
                    }
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
                
                if (!item.isBought && cartItems.Contains(item))
                {
                    ItemData itemData = item.GetItemData();
                    if (cartItems.Remove(item))
                    {
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
        }

        public async void TryPurchaseItems()
        {
            if (cartItems.Count == 0) return;
            
            if (ShopManager.Instance.TryPurchase(cartItems))
            {
                // Add cart items to a new list (to keep track when all items had been taken from cart after purchase)
                foreach (Item item in cartItems)
                {
                    // This is a hay block - don't add it to bought items list
                    if (item.GetItemData().itemName.Equals("Fortress Defense"))
                    {
                        continue;
                    }
                    
                    // Add every other item
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
