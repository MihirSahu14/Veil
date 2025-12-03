using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class RoomSpawner : MonoBehaviour
{
    [Header("Spawn Area")]
    public BoxCollider2D roomArea;

    // [Header("Drawer settings")]
    // public GameObject drawerPrefab;
    // [Min(0)] public int drawersPerRoom = 2;
    // public float drawerRadius = 0.35f;

    [Header("Enemy settings")]
    public GameObject enemyPrefab;
    public bool spawnEnemy = true;
    public float enemyRadius = 0.35f;
    [Tooltip("Minimum distance between enemy spawn and player start.")]
    public float enemyMinDistanceFromPlayer = 2.0f;

    [Header("Battery settings")]
    public GameObject batteryPrefab;
    [Min(0)] public int minBatteries = 0;
    [Min(0)] public int maxBatteries = 2;
    public float batteryRadius = 0.25f;

    [Header("Collision avoidance (optional)")]
    public LayerMask blockedMask;           // walls / props, NOT the floor
    public int maxTriesPerSpawn = 25;

    public float enemySpawnDelay = 5f;

    void Awake()
    {
        if (!roomArea) roomArea = GetComponent<BoxCollider2D>();
    }

    void Start()
    {
        if (!roomArea)
        {
            Debug.LogWarning("[RoomSpawner] No roomArea assigned.", this);
            return;
        }

        // 1) Enemy (spawn later)
        if (spawnEnemy && enemyPrefab)
        {
            StartCoroutine(SpawnEnemyAfterDelay());
        }

        // 2) Batteries (spawn immediately)
        if (batteryPrefab && maxBatteries >= 0)
        {
            int count = Random.Range(minBatteries, maxBatteries + 1);
            for (int i = 0; i < count; i++)
            {
                Vector2 pos = PickSpawnPosition(batteryRadius);
                Instantiate(batteryPrefab, pos, Quaternion.identity);
            }
        }

        // 3) Drawers are currently commented out – you can re-enable them here if needed
    }


    IEnumerator SpawnEnemyAfterDelay()
    {
        // wait in game time (respects pause)
        yield return new WaitForSeconds(enemySpawnDelay);

        Vector2 playerPos = GetPlayerPosition();
        Vector2 pos = PickSpawnPositionEnemy(enemyRadius, playerPos, enemyMinDistanceFromPlayer);
        Instantiate(enemyPrefab, pos, Quaternion.identity);
    }

    // -------- generic spawn (no player distance) --------
    Vector2 PickSpawnPosition(float radius)
    {
        Bounds b = roomArea.bounds;
        Vector2 chosen = b.center;  // instead of roomArea.transform.position

        for (int attempt = 0; attempt < maxTriesPerSpawn; attempt++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            Vector2 candidate = new Vector2(x, y);

            if (blockedMask.value != 0)
            {
                Collider2D hit = Physics2D.OverlapCircle(candidate, radius, blockedMask);
                if (hit != null) continue;
            }

            chosen = candidate;
            break;
        }

        return chosen;
    }

    Vector2 PickSpawnPositionEnemy(float radius, Vector2 playerPos, float minDist)
    {
        Bounds b = roomArea.bounds;
        Vector2 chosen = b.center;  // instead of roomArea.transform.position

        for (int attempt = 0; attempt < maxTriesPerSpawn; attempt++)
        {
            float x = Random.Range(b.min.x, b.max.x);
            float y = Random.Range(b.min.y, b.max.y);
            Vector2 candidate = new Vector2(x, y);

            if (blockedMask.value != 0)
            {
                Collider2D hit = Physics2D.OverlapCircle(candidate, radius, blockedMask);
                if (hit != null) continue;
            }

            if ((playerPos - candidate).sqrMagnitude < minDist * minDist)
                continue; // too close to player

            chosen = candidate;
            break;
        }

        return chosen;
    }

    Vector2 GetPlayerPosition()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        return player ? (Vector2)player.transform.position : roomArea.bounds.center;
    }

    void OnDrawGizmosSelected()
    {
        if (!roomArea) roomArea = GetComponent<BoxCollider2D>();
        if (!roomArea) return;

        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        Gizmos.DrawCube(roomArea.bounds.center, roomArea.bounds.size);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(roomArea.bounds.center, roomArea.bounds.size);
    }
}
