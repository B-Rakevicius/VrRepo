using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(AudioSource))]
    public class GameOverUI : MonoBehaviour
    {
        [Header("UI References")] 
        [Tooltip("A reference to the GameOver Image object")]
        [SerializeField] private RectTransform gameOverHeader;
    
        [Tooltip("A reference to the DiedToPanel object")]
        [SerializeField] private RectTransform diedToPanel;
    
        [Tooltip("A reference to the FinalScorePanel object")]
        [SerializeField] private RectTransform finalScorePanel;

        [Header("Buttons")]
        [Tooltip("A reference to RetryButton")]
        [SerializeField] private Button retryButton;
    
        [Tooltip("A reference to QuitButton")]
        [SerializeField] private Button quitButton;
    
        [Header("Reveal timings")]
        [Tooltip("How long to wait till GameOver Image is displayed in seconds")]
        [SerializeField] private int gameOverHeaderRevealTiming = 1;
    
        [Tooltip("How long to wait till DiedToPanel is displayed in seconds")]
        [SerializeField] private int diedToPanelRevealTiming = 1;
    
        [Tooltip("How long to wait till FinalScorePanel is displayed in seconds")]
        [SerializeField] private int finalScorePanelRevealTiming = 1;
    
        [Tooltip("How long to wait till Buttons are displayed in seconds")]
        [SerializeField] private int buttonsRevealTiming = 1;

        [Header("Audio Settings")] 
        [Tooltip("What sound to play when each UI item is revealed")]
        [SerializeField] private AudioClip uiRevealSound;
        
        private AudioSource _audioSource;
        private float _timeTillNextReveal;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        
        private void Start()
        {
            // Add listeners for buttons
            
            retryButton.onClick.AddListener(OnRetryButtonClicked);
            quitButton.onClick.AddListener(() => { Application.Quit(); });
            
            // When instantiated, keep everything hidden on start
            HideQuitButton();
            HideRetryButton();
            HideFinalScorePanel();
            HideDiedToPanel();
            HideGameOverHeader();
        
            // Start revealing UI elements 
            RevealUIElements();
        }

        private void OnRetryButtonClicked()
        {
            // Restart the game
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Slowly reveals all UI elements after their specified reveal time
        /// </summary>
        private async void RevealUIElements()
        {
            // Reveal Game over header
            await Task.Delay(gameOverHeaderRevealTiming*1000);
            ShowGameOverHeader();
            PlayUIRevealSound();

            // Reveal DiedTo panel
            await Task.Delay(diedToPanelRevealTiming*1000);
            ShowDiedToPanel();
            PlayUIRevealSound();

            // Reveal FinalScore panel
            await Task.Delay(finalScorePanelRevealTiming*1000);
            ShowFinalScorePanel();
            PlayUIRevealSound();

            // Reveal buttons
            await Task.Delay(buttonsRevealTiming*1000);
            ShowRetryButton();
            ShowQuitButton();
            PlayUIRevealSound();

        }

        private void PlayUIRevealSound()
        {
            if (uiRevealSound != null)
            {
                _audioSource.PlayOneShot(uiRevealSound);
            }
            else
            {
                Debug.LogError("UI Reveal Sound is not set in GameOver UI!");
            }
        }
    
        #region Show UI Elements
        private void ShowGameOverHeader()
        {
            gameOverHeader.gameObject.SetActive(true);
        }

        private void ShowDiedToPanel()
        {
            diedToPanel.gameObject.SetActive(true);
        }

        private void ShowFinalScorePanel()
        {
            finalScorePanel.gameObject.SetActive(true);
        }

        private void ShowRetryButton()
        {
            retryButton.gameObject.SetActive(true);
        }

        private void ShowQuitButton()
        {
            quitButton.gameObject.SetActive(true);
        }
        #endregion
    
        #region Hide UI Elements
        private void HideGameOverHeader()
        {
            gameOverHeader.gameObject.SetActive(false);
        }

        private void HideDiedToPanel()
        {
            diedToPanel.gameObject.SetActive(false);
        }

        private void HideFinalScorePanel()
        {
            finalScorePanel.gameObject.SetActive(false);
        }

        private void HideRetryButton()
        {
            retryButton.gameObject.SetActive(false);
        }

        private void HideQuitButton()
        {
            quitButton.gameObject.SetActive(false);
        }
        #endregion
    }
}
