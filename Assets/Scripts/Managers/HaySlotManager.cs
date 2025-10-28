using System.Collections.Generic;
using UnityEngine;
public class HaySlotManager : MonoBehaviour
{
    public static HaySlotManager Instance;
    [Header("Slot Settings")]
    [SerializeField] private List<Transform> haySlots = new List<Transform>();
    [SerializeField] private GameObject hayPrefab;
    private Dictionary<Transform, GameObject> slotOccupancy = new Dictionary<Transform, GameObject>();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        InitializeSlots();
    }
    private void InitializeSlots()
    {
        foreach (Transform slot in haySlots)
        {
            slotOccupancy[slot] = null;
        }
        // Spawn 4 hay blocks at 12, 3, 6, and 9 o’clock (slots 0, 2, 4, 6)
        for (int i = 0; i < haySlots.Count; i++)
        {
            if (i % 2 == 0)
            {
                PlaceHayInSlot(haySlots[i]);
            }
        }
    }
    public void PlaceHayInSlot(Transform slot)
    {
        if (slot == null || slotOccupancy[slot] != null)
        {
            Debug.LogWarning("Slot is already occupied or invalid!");
            return;
        }

        GameObject hay = Instantiate(hayPrefab, slot.position, slot.rotation);
        slotOccupancy[slot] = hay;
    }
    public Transform GetFreeSlot()
    {
        foreach (var slot in haySlots)
        {
            if (slotOccupancy[slot] == null)
                return slot;
        }
        return null;
    }
    public void OnHayDestroyed(GameObject hay)
    {
        foreach (var kvp in slotOccupancy)
        {
            if (kvp.Value == hay)
            {
                slotOccupancy[kvp.Key] = null;
                break;
            }
        }
        if (AllSlotsEmpty())
        {
            GameManager.Instance.EndRound(); // or Game Over
        }
    }
    private bool AllSlotsEmpty()
    {
        foreach (var kvp in slotOccupancy)
        {
            if (kvp.Value != null)
                return false;
        }
        return true;
    }
}
