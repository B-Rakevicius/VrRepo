using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEditor.VersionControl;
using UnityEngine;
using System.Threading.Tasks;
using Managers;
using Player;
using Task = UnityEditor.VersionControl.Task;

namespace Shop
{
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance;
        
        [Header("Shop Items Pool")]
        [Tooltip("All available shop items to buy.")]
        [SerializeField] private List<ItemData> items;
        
        [Header("Shop Prefab")]
        [SerializeField] private Transform shopPrefab;
        
        private Transform _spawnedShop; // Keep the reference to be able to destroy the shop.

        public event EventHandler<OnItemPoolReceivedEventArgs> OnItemPoolReceived;
        public class OnItemPoolReceivedEventArgs : EventArgs {
            public List<ItemData> items { get; set; }
        }

        public event EventHandler OnShopAnimationFinished;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("There is more than one ShopManager instance!");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
        }
        
        private void Start()
        {
            // Subscribe to GameManager events
            GameManager.Instance.OnRoundStarted += GameManager_RoundStarted;
            GameManager.Instance.OnRoundEnded += GameManager_RoundEnded;
        }
        
        private void OnDestroy()
        {
            GameManager.Instance.OnRoundStarted -= GameManager_RoundStarted;
            GameManager.Instance.OnRoundEnded -= GameManager_RoundEnded;
        }

        private void GameManager_RoundStarted(object sender, EventArgs e)
        {
            if (_spawnedShop is null) { return; }
            
            Destroy(_spawnedShop.gameObject);
        }
        
        private void GameManager_RoundEnded(object sender, EventArgs e)
        {
            SpawnShop();
        }

        private async void SpawnShop()
        {
            _spawnedShop = Instantiate(shopPrefab);
            bool finished = await _spawnedShop.GetComponent<ShopAnimator>().AnimateShopFall();
            if (finished)
            {
                GetItemPool();
            }
        }
        private void GetItemPool()
        {
            List<ItemData> itemPool = items.FindAll(x => x.unlocksAt <= GameManager.Instance.currentWave);
            
            OnItemPoolReceived?.Invoke(this, new OnItemPoolReceivedEventArgs { items = itemPool });
        }

        public bool TryPurchase(List<Item> shoppingCartItems)
        {
            int totalPrice = shoppingCartItems.Sum(x => x.GetItemData().itemPrice);
            
            // Check if player has enough money
            if (PlayerManager.Instance.HasEnoughMoney(totalPrice))
            {
                // Loop through all items and buy them. Also mark them as bought
                foreach (Item item in shoppingCartItems)
                {
                    item.IsBought = true;
                }
                
                PlayerManager.Instance.DeductMoney(totalPrice);
                
                return true;
            }
            else
            {
                return false;
            }
        }
        
        
    }
}
