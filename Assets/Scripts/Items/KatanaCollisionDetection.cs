using UnityEngine;

namespace Items
{
    public class KatanaCollisionDetection : MonoBehaviour
    {
        [SerializeField] private MeleeWeapon weapon;
        private void OnTriggerEnter(Collider other)
        {
            weapon.DetectHits(other);
        }
    }
}
