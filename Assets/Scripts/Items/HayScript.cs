using UnityEngine;

public class HayScript : MonoBehaviour
{
    [SerializeField] private int maxHealth = 3;
    public int currentHealth;
    [SerializeField] private MeshRenderer meshRenderer;
    void Start()
    {
        currentHealth = maxHealth;
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisuals();
    }
    public void TakeBite()
    {
        currentHealth--;
        UpdateVisuals();

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
    // delete after, needed for troubleshooting size visual change
    private void Update()
    {
        UpdateVisuals();
    }
    private void UpdateVisuals()
    {
        float scale = 1.5f + (currentHealth / (float)maxHealth ) * 0.5f;
        transform.localScale = Vector3.one * scale;
    }
    public bool IsDestroyed()
    {
        return currentHealth <= 0;
    }
}