using System;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        
        [SerializeField] private int health = 3;
        [SerializeField] private int currentMoney = 20;
        [SerializeField] private int maxMoney = 100;
        
        public int CurrentMoney => currentMoney;
        public int MaxMoney => maxMoney;

        public event EventHandler<OnMoneyChangedEventArgs> OnMoneyChanged;
        public class OnMoneyChangedEventArgs : EventArgs
        {
            public int Money { get; set; }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("There is more than one PlayerManager instance!");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            FindHaySetHealth();
        }

        private void Start()
        {
            // Invoke the event so that liquid shader can properly update
            OnMoneyChanged?.Invoke(this, new OnMoneyChangedEventArgs { Money = currentMoney });
        }
        
        private void FindHaySetHealth()
        {
            health = UnityEngine.Object.FindObjectsByType<HayScript>(FindObjectsSortMode.None).Length;
        }
        public void DeductMoney(int amount)
        {
            currentMoney -= amount;
            currentMoney = Mathf.Clamp(currentMoney, 0, maxMoney);
            OnMoneyChanged?.Invoke(this, new OnMoneyChangedEventArgs { Money = currentMoney });
        }

        public void AddMoney(int amount)
        {
            currentMoney += amount;
            currentMoney = Mathf.Clamp(currentMoney, 0, maxMoney);
            OnMoneyChanged?.Invoke(this, new OnMoneyChangedEventArgs { Money = currentMoney });
        }

        /// <summary>
        /// Checks whether the player has enough money.
        /// </summary>
        /// <param name="amount">Amount to compare against.</param>
        public bool HasEnoughMoney(int amount)
        {
            return amount <= currentMoney;
        }
    }

}
