using Items;
using Unity.Mathematics;
using UnityEngine;

public class CrossbowArrowInsertTrigger : MonoBehaviour
{
    private CrossBuh crossBuh;

    private void Start()
    {
        crossBuh = GetComponentInParent<CrossBuh>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow"))
        {
            if (crossBuh.TryReload())
            {
                // Deattach arrow from the player. Maybe just destroy that arrow and instantiate anew?
                Destroy(other.gameObject);
            }
        }
    }
}
