using System.Threading.Tasks;
using UnityEngine;

namespace UI
{
    public class AccessibilityUI : MonoBehaviour
    {
        public static AccessibilityUI Instance;

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
        
        public bool IsActive()
        {
            return gameObject.activeSelf;
        }
    }
}
