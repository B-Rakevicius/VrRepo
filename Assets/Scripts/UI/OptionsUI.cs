using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class OptionsUI : MonoBehaviour
    {
        public static OptionsUI Instance;

        [Header("Options Buttons")] 
        [SerializeField] private Button volumeButton;
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Button accessibilityButton;
        [SerializeField] private Button backButton;

        [Header("Animation Settings")] 
        [SerializeField] private float animDuration = 1f;
        [SerializeField] private float animStartScale = 0f;
        [SerializeField] private float animEndScale = 1f;

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
            
            gameObject.SetActive(false);
        }

        private void Start()
        {
            volumeButton.onClick.AddListener(OnVolumeButtonClicked);
            graphicsButton.onClick.AddListener(OnGraphicsButtonClicked);
            accessibilityButton.onClick.AddListener(OnAccessibilityButtonClicked);
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        private void OnBackButtonClicked()
        {
            // Hide sub-options UI if active
            if (VolumeUI.Instance.IsActive())
                VolumeUI.Instance.HideWithAnim();

            if (GraphicsUI.Instance.IsActive())
                GraphicsUI.Instance.HideWithAnim();

            if (AccessibilityUI.Instance.IsActive())
                AccessibilityUI.Instance.HideWithAnim();
                        
            // Hide options UI
            HideWithAnim();
        }

        private void OnAccessibilityButtonClicked()
        {
            if (AccessibilityUI.Instance.IsActive()) { return; }

            // Instantly hide other UI
            GraphicsUI.Instance.Hide();
            VolumeUI.Instance.Hide();
            
            // Show Accessibility UI
            AccessibilityUI.Instance.Show();
        }

        private void OnGraphicsButtonClicked()
        {
            if (GraphicsUI.Instance.IsActive()) { return; }
            
            // Instantly hide other UI
            AccessibilityUI.Instance.Hide();
            VolumeUI.Instance.Hide();

            // Show Graphics UI
            GraphicsUI.Instance.Show();
        }

        private void OnVolumeButtonClicked()
        {
            if (VolumeUI.Instance.IsActive()) { return; }
            
            // Instantly hide other UI
            GraphicsUI.Instance.Hide();
            AccessibilityUI.Instance.Hide();
            
            // Show Volume UI
            VolumeUI.Instance.Show();
        }

        public async void Show()
        {
            gameObject.SetActive(true);
            
            await UIAnimator.ScaleAnim(GetComponent<RectTransform>(), animDuration, animStartScale, animEndScale);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public async void HideWithAnim()
        {
            await UIAnimator.ScaleAnim(GetComponent<RectTransform>(), animDuration, animEndScale, animStartScale);

            Hide();
        }
        
        public bool IsActive()
        {
            return gameObject.activeSelf;
        }
    }
}
