using System.Threading.Tasks;
using UnityEngine;

namespace Utils
{
    public static class UIAnimator
    {
        public static async Task ScaleAnim(RectTransform target, float animDuration, float animStartScale, float animEndScale)
        {
            float currentAnimTime = 0f;

            while (currentAnimTime <= animDuration)
            {
                // Normalize to 0-1 range
                float t = currentAnimTime / animDuration;
                
                float newScale = Mathf.Lerp(animStartScale, animEndScale, t);
                Vector3 newScaleVector = new Vector3(newScale, newScale, newScale);
                target.localScale = newScaleVector;
                
                currentAnimTime += Time.deltaTime;
                
                await Task.Yield();
            }
            
            // Sometimes the timer cannot reach 1, so make sure the scale has reached its value.
            target.localScale = new Vector3(animEndScale, animEndScale, animEndScale);
        }
    }
}
