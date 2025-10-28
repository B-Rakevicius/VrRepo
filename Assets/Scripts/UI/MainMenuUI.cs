using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        public static MainMenuUI Instance;

        [Header("Main Menu Buttons")] 
        [SerializeField] private Button startButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
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
            startButton.onClick.AddListener(OnStartButtonClicked);
            optionsButton.onClick.AddListener(OnOptionsButtonClicked);
            quitButton.onClick.AddListener(() => { Application.Quit(); });
        }
        
        private void OnOptionsButtonClicked()
        {
            if (OptionsUI.Instance.IsActive()) { return;}
            
            OptionsUI.Instance.Show();
        }

        private void OnStartButtonClicked()
        {
            // Start the game
        }
    }
}
