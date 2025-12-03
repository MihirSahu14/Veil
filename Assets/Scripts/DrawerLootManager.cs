using UnityEngine;

public class DrawerLootManager : MonoBehaviour
{
    [Header("Loot chances for NON-key drawers")]
    [Range(0f, 1f)]
    public float batteryChance = 0.5f;   // chance a non-key drawer has a battery

    void Start()
    {
        // Find all drawers that exist after all spawners have run
        Drawer[] drawers = Object.FindObjectsByType<Drawer>(FindObjectsSortMode.None);
        if (drawers.Length == 0)
        {
            Debug.LogWarning("[DrawerLootManager] No drawers found in scene.");
            return;
        }

        // Pick ONE drawer at random to hold the key
        int keyIndex = Random.Range(0, drawers.Length);

        for (int i = 0; i < drawers.Length; i++)
        {
            if (i == keyIndex)
            {
                drawers[i].lootType = DrawerLootType.Key;
            }
            else
            {
                // Battery vs empty for non-key drawers
                bool giveBattery = Random.value < batteryChance;
                drawers[i].lootType = giveBattery ? DrawerLootType.Battery : DrawerLootType.None;
            }
        }

        Debug.Log($"[DrawerLootManager] Assigned key to drawer {keyIndex} out of {drawers.Length}");
    }
}
