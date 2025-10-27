using Player;
using UnityEngine;

namespace Items
{
    [RequireComponent(typeof(Rigidbody))]
    public class Orb : MonoBehaviour
    {
        [Tooltip("How much money is the orb worth.")]
        [SerializeField] private int value = 1;
        
        public int Value => value;

        public void Collect()
        {
            // Add money
            PlayerManager.Instance.AddMoney(value);
            
            // TODO: Add particles, sfx?
            
            Destroy(gameObject);
        }
    }
}
