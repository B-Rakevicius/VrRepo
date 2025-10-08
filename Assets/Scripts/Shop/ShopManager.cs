using System;
using System.Collections.Generic;
using Items;
using UnityEditor.VersionControl;
using UnityEngine;
using System.Threading.Tasks;
using Task = UnityEditor.VersionControl.Task;

namespace Shop
{
    public class ShopManager : MonoBehaviour
    {
        
        [SerializeField] private List<ItemData> items;
        
        [SerializeField] private Transform shopPrefab;
        [SerializeField] private Transform shopSpawnPoint;
        private Transform _spawnedShop;
        
        private int _currentRound;

        public static event EventHandler<OnItemPoolReceivedEventArgs> OnItemPoolReceived;
        public class OnItemPoolReceivedEventArgs : EventArgs
        {
            public List<ItemData> items { get; set; }
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
            _currentRound = GameManager.Instance.currentRound;
            
            // TODO: Create a shop falling animation. For now I will just instantiate it at fixed position.
            _spawnedShop = Instantiate(shopPrefab, shopSpawnPoint.position, shopSpawnPoint.rotation);

            await System.Threading.Tasks.Task.Delay(1000);
            
            // Get item pool for current round
            GetItemPool();
        }

        private void GetItemPool()
        {
            List<ItemData> itemPool = items.FindAll(x => x.unlocksAt <= _currentRound);
            
            OnItemPoolReceived?.Invoke(this, new OnItemPoolReceivedEventArgs { items = itemPool });
        }
        
        
    }
}
