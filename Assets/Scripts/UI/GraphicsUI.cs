using System.Threading.Tasks;
using UnityEngine;
using Utils;

namespace UI
{
    public class GraphicsUI : MonoBehaviour
    {
        public static GraphicsUI Instance;
        
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
