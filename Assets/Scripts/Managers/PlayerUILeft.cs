using System;
using Player;
using TMPro;
using UnityEngine;

namespace Managers
{
    public class PlayerUILeft : MonoBehaviour
    {
        public static PlayerUILeft Instance;

        [SerializeField] private TextMeshProUGUI moneyText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("There is more than one PlayerUI instance!");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            PlayerManager.Instance.OnMoneyChanged += PlayerManager_MoneyChanged;
            UpdateMoneyText(PlayerManager.Instance.CurrentMoney);
        }

        private void OnDestroy()
        {
            PlayerManager.Instance.OnMoneyChanged -= PlayerManager_MoneyChanged;
        }

        private void PlayerManager_MoneyChanged(object sender, PlayerManager.OnMoneyChangedEventArgs e)
        {
            UpdateMoneyText(e.Money);
        }

        public void UpdateMoneyText(int money)
        {
            moneyText.text = "Money: " + money;
        }
    }
}
