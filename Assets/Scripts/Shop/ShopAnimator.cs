using UnityEngine;
using System.Threading.Tasks;

namespace Shop
{
    public class ShopAnimator : MonoBehaviour
    {
        [Header("Shop Animation Settings")]
        [SerializeField] private Transform shopSpawnPoint;
        [SerializeField] private Transform shopLandingSpot;
        [SerializeField] private float animDuration = 0.3f;
        
        [SerializeField] private ParticleSystem smokeParticles;
        
        
        public async Task<bool> AnimateShopFall()
        {
            float currentDuration = 0f;
            Vector3 shopSpawnPointWS = transform.TransformPoint(shopSpawnPoint.position);
            Vector3 shopLandingSpotWS = transform.TransformPoint(shopLandingSpot.position);

            while (currentDuration < animDuration)
            {
                // Do the falling
                Vector3 newPosition = Vector3.Lerp(shopSpawnPointWS, shopLandingSpotWS, currentDuration / animDuration);
                transform.position = newPosition;
                currentDuration += Time.deltaTime;
                await Task.Yield();
            }
            
            // Set the final position just to be sure it is in the expected position
            transform.position = shopLandingSpotWS;
            
            // Activate dust particles
            smokeParticles.Play();
            
            return true;
        }
    }
}
