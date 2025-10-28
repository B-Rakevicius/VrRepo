using System.Threading.Tasks;
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
        
        [Header("Animation Settings")] 
        [SerializeField] private float animDuration = 0.2f;
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
            GameManager.Instance.StartGame();
            
            // Hide every Menu UI that is visible
            if (VolumeUI.Instance.IsActive())
                VolumeUI.Instance.HideWithAnim();

            if (GraphicsUI.Instance.IsActive())
                GraphicsUI.Instance.HideWithAnim();

            if (AccessibilityUI.Instance.IsActive())
                AccessibilityUI.Instance.HideWithAnim();
            
            if(OptionsUI.Instance.IsActive())
                OptionsUI.Instance.HideWithAnim();
                        
            // Hide options UI
            HideWithAnim();
        }
        
        private async void ScaleInAnim()
        {
            float currentAnimTime = 0f;
            Debug.Log("Entered ScaleInAnim");

            while (currentAnimTime <= animDuration)
            {
                // Normalize to 0-1 range
                float t = currentAnimTime / animDuration;

                Debug.Log($"AnimTime: {t}");
                
                float newScale = Mathf.Lerp(animStartScale, animEndScale, t);
                Vector3 newScaleVector = new Vector3(newScale, newScale, newScale);
                transform.localScale = newScaleVector;
                
                currentAnimTime += Time.deltaTime;
                
                await Task.Yield();
            }
            
            // Sometimes the timer cannot reach 1, so make sure the scale has reached its value.
            transform.localScale = new Vector3(animEndScale, animEndScale, animEndScale);
        }
        
        private async void ScaleOutAnim()
        {
            float currentAnimTime = 0f;
            Debug.Log("Entered ScaleInAnim");

            while (currentAnimTime <= animDuration)
            {
                // Normalize to 0-1 range
                float t = currentAnimTime / animDuration;

                Debug.Log($"AnimTime: {t}");
                
                float newScale = Mathf.Lerp(animEndScale, animStartScale, t);
                Vector3 newScaleVector = new Vector3(newScale, newScale, newScale);
                transform.localScale = newScaleVector;
                
                currentAnimTime += Time.deltaTime;
                
                await Task.Yield();
            }
            
            // Sometimes the timer cannot reach 1, so make sure the scale has reached its value.
            transform.localScale = new Vector3(animStartScale, animStartScale, animStartScale);
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            
            ScaleInAnim();
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void HideWithAnim()
        {
            ScaleOutAnim();
        }
    }
}
