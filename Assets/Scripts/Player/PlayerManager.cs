using System;
using UnityEngine;

namespace Player
{
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance;
        
        [SerializeField] private int health = 3;
        [SerializeField] private int currentMoney = 20;

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
        }

        public void DeductMoney(int amount)
        {
            currentMoney -= amount;
            OnMoneyChanged?.Invoke(this, new OnMoneyChangedEventArgs { Money = currentMoney });
        }

        public void AddMoney(int amount)
        {
            currentMoney += amount;
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

        public int GetCurrentMoney()
        {
            return currentMoney;
        }
    }
}
