using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using UnityEngine;
using System.Threading.Tasks;
using Managers;
using Player;
using UI;

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
        [Header("HayManager")]
        [SerializeField] private HaySlotManager haySlotManager;
        private Transform _spawnedShop; // Keep the reference to be able to destroy the shop.
        private List<GameObject> spawnedShopItems = new List<GameObject>(); // A list of currently spawned items. Is used to be able to destroy not bought items after round starts.
        
        [Header("Sound Settings")]
        [Tooltip("What sound should shop play when item is bought?")]
        [SerializeField] private AudioClip audioClip;
        
        private AudioSource audioSource;
        
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
                audioSource = GetComponent<AudioSource>();
                //DontDestroyOnLoad(this);
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
            Grenade[] grenades = FindObjectsOfType<Grenade>();
            foreach (Grenade grenade in grenades)
            {
                grenade.SetShopState(false);
            }
            DecoyGrenade[] grenades2 = FindObjectsOfType<DecoyGrenade>();
            foreach (DecoyGrenade grenade in grenades2)
            {
                grenade.SetShopState(false);
            }
            // Destroy the shop
            Destroy(_spawnedShop.gameObject);
            
            // Destroy items that were not bought
            DestroyNotBoughtItems();
        }
        private void GameManager_RoundEnded(object sender, EventArgs e)
        {
            SpawnShop();
        }
        
        private async void SpawnShop()
        {
            if (_spawnedShop != null) return;
            
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
            Grenade[] grenades = FindObjectsOfType<Grenade>();
            foreach (Grenade grenade in grenades)
            {
                grenade.SetShopState(true);
            }
            DecoyGrenade[] grenades2 = FindObjectsOfType<DecoyGrenade>();
            foreach (DecoyGrenade grenade in grenades2)
            {
                grenade.SetShopState(false);
            }
        }
        public bool TryPurchase(HashSet<Item> shoppingCartItems)
        {
            int totalPrice = shoppingCartItems.Sum(x => x.GetItemData().itemPrice);
            // Check if player has enough money
            Debug.Log("Has enough money: " + PlayerManager.Instance.HasEnoughMoney(totalPrice));
            if (PlayerManager.Instance.HasEnoughMoney(totalPrice))
            {
                // Loop through all items and buy them. Also mark them as bought
                foreach (Item item in shoppingCartItems)
                {
                    // Disable shop item UI
                    item.GetComponent<ItemUI>().HideUI();
                    
                    item.IsBought = true;

                    if (item.GetItemData().itemName == "HayItem")
                    {
                        Transform freeSlot = HaySlotManager.Instance.GetFreeSlot();
                        if (freeSlot != null)
                        {
                            HaySlotManager.Instance.PlaceHayInSlot(freeSlot);
                        }
                        else
                        {
                            Debug.LogError("No free hay slots available!");
                        }
                    }
                    else
                    {
                        Debug.LogError("nohaye!");
                    }
                }
                
                PlayerManager.Instance.DeductMoney(totalPrice);
                PlayShopSound();
                
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PlayShopSound()
        {
            if (audioClip != null)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }
        
        /// <summary>
        /// Destroys shop items that weren't bought.
        /// </summary>
        private void DestroyNotBoughtItems()
        {
            foreach (GameObject itemObject in spawnedShopItems)
            {
                if (itemObject.TryGetComponent(out Item item))
                {
                    if (!item.IsBought)
                    {
                        Destroy(itemObject);
                    }
                }
            }
            spawnedShopItems.Clear();
        }

        /// <summary>
        /// Adds newly spawned shop items to the list.
        /// </summary>
        /// <param name="spawnedItem"></param>
        public void AddSpawnedShopItem(GameObject spawnedItem)
        {
            spawnedShopItems.Add(spawnedItem);
        }
    }
}
