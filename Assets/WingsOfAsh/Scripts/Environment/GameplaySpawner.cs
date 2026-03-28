using UnityEngine;
using TMPro;

public class GameplaySpawner : MonoBehaviour
{
    [Header("Parents (must match WorldScroller roots)")]
    [SerializeField] private Transform environmentRoot;
    [SerializeField] private Transform enemiesRoot;
    [SerializeField] private Transform pickupsRoot;

    [Header("Player")]
    [SerializeField] private Transform playerTransform;

    [Header("Prefabs")]
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject polePrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject emberHeartPrefab;
    [SerializeField] private GameObject enemyShotPrefab;

    [Header("Timing (seconds)")]
    [SerializeField] private float initialDelay = 1.25f;
    [SerializeField] private float gateInterval = 5.2f;
    [SerializeField] private Vector2 gateIntervalJitter = new Vector2(-0.4f, 0.8f);
    [SerializeField] private float poleInterval = 9f;
    [SerializeField] private float enemyInterval = 4.75f;
    [SerializeField] private float heartInterval = 7f;
    [SerializeField] private float enemyIntervalMin = 2.2f;

    [Header("Spawn position")]
    [Tooltip("World X offset from player where objects appear (ahead of the dragon).")]
    [SerializeField] private float spawnAheadOfPlayer = 14f;
    [SerializeField] private float spawnAheadJitter = 0.75f;
    [SerializeField] private float yMin = -3.4f;
    [SerializeField] private float yMax = 3.4f;
    [SerializeField] private Vector2 poleYRange = new Vector2(-3.9f, -2.1f);
    [SerializeField] private float minBreathingDistanceFromPlayer = 10f;
    [SerializeField] private float minDistanceBetweenEnvironmentSpawns = 5.5f;
    [SerializeField] private float minDistanceBetweenAnySpawns = 1.8f;

    [Header("Polish")]
    [SerializeField] private bool addScrollDespawnToGates = true;

    [Header("Spawn Collision Avoidance")]
    [SerializeField] private bool avoidOverlapOnSpawn = true;
    [SerializeField] private float enemySpawnClearRadius = 0.7f;
    [SerializeField] private float heartSpawnClearRadius = 0.6f;
    [SerializeField] private int spawnPositionRetryCount = 8;

    [Header("Enemy Difficulty Scaling")]
    [SerializeField] private int enemySpawnCountStart = 1;
    [SerializeField] private int enemySpawnCountMax = 5;
    [SerializeField] private float secondsPerEnemyCountStep = 25f;
    [SerializeField] private float horizontalSpacingBetweenEnemySpawns = 1f;
    [SerializeField] private TMP_Text enemyCountText;

    private float gateCountdown;
    private float poleCountdown;
    private float enemyCountdown;
    private float heartCountdown;
    private FlameHealth playerHealth;
    private float difficultyElapsed;
    private float lastEnvironmentSpawnX = float.NegativeInfinity;
    private float lastAnySpawnX = float.NegativeInfinity;

    private void Start()
    {
        if (playerTransform == null)
        {
            FlameHealth hp = FindFirstObjectByType<FlameHealth>();
            if (hp != null)
            {
                playerTransform = hp.transform;
                playerHealth = hp;
            }
        }
        else if (playerHealth == null)
        {
            playerHealth = playerTransform.GetComponent<FlameHealth>();
            if (playerHealth == null)
            {
                playerHealth = playerTransform.GetComponentInChildren<FlameHealth>();
            }
        }

        gateCountdown = initialDelay;
        poleCountdown = initialDelay + 2f;
        enemyCountdown = initialDelay + 0.5f;
        heartCountdown = initialDelay + 1.25f;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (playerHealth == null)
        {
            playerHealth = playerTransform.GetComponent<FlameHealth>();
            if (playerHealth == null)
            {
                playerHealth = playerTransform.GetComponentInChildren<FlameHealth>();
            }
        }

        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        difficultyElapsed += Time.deltaTime;
        gateCountdown -= Time.deltaTime;
        poleCountdown -= Time.deltaTime;
        enemyCountdown -= Time.deltaTime;
        heartCountdown -= Time.deltaTime;

        if (gateCountdown <= 0f && gatePrefab != null && environmentRoot != null)
        {
            SpawnGate();
            float jitterMin = Mathf.Min(gateIntervalJitter.x, gateIntervalJitter.y);
            float jitterMax = Mathf.Max(gateIntervalJitter.x, gateIntervalJitter.y);
            float next = gateInterval + Random.Range(jitterMin, jitterMax);
            gateCountdown = Mathf.Max(0.5f, next);
        }

        if (poleCountdown <= 0f && polePrefab != null && environmentRoot != null)
        {
            SpawnPole();
            poleCountdown = poleInterval + Random.Range(-0.35f, 0.35f);
        }

        if (enemyCountdown <= 0f && enemyPrefab != null && enemiesRoot != null)
        {
            int spawnCount = GetCurrentEnemySpawnCount();
            SpawnEnemyBurst(spawnCount);
            UpdateEnemyCountUI(spawnCount);
            float scaledInterval = Mathf.Max(enemyIntervalMin, enemyInterval - (spawnCount - enemySpawnCountStart) * 0.25f);
            enemyCountdown = scaledInterval + Random.Range(-0.2f, 0.2f);
        }

        if (heartCountdown <= 0f && emberHeartPrefab != null && pickupsRoot != null)
        {
            SpawnHeart();
            heartCountdown = heartInterval + Random.Range(-0.25f, 0.25f);
        }
    }

    private float GetSpawnX(bool environmentSpawn)
    {
        float jitter = Random.Range(-spawnAheadJitter, spawnAheadJitter);
        float baseX = playerTransform.position.x + spawnAheadOfPlayer + jitter;
        float minFromPlayer = playerTransform.position.x + minBreathingDistanceFromPlayer;
        float minFromAny = lastAnySpawnX + minDistanceBetweenAnySpawns;

        float x = Mathf.Max(baseX, minFromPlayer, minFromAny);
        if (environmentSpawn)
        {
            float minFromEnv = lastEnvironmentSpawnX + minDistanceBetweenEnvironmentSpawns;
            x = Mathf.Max(x, minFromEnv);
            lastEnvironmentSpawnX = x;
        }

        lastAnySpawnX = x;
        return x;
    }

    private void SpawnGate()
    {
        float y = Random.Range(yMin, yMax);
        Vector3 pos = new Vector3(GetSpawnX(true), y, 0f);
        GameObject instance = Instantiate(gatePrefab, pos, Quaternion.identity, environmentRoot);

        if (addScrollDespawnToGates && instance.GetComponent<ScrollDespawn>() == null)
        {
            instance.AddComponent<ScrollDespawn>();
        }
    }

    private void SpawnPole()
    {
        float minPoleY = Mathf.Min(poleYRange.x, poleYRange.y);
        float maxPoleY = Mathf.Max(poleYRange.x, poleYRange.y);
        float y = Random.Range(minPoleY, maxPoleY);
        Vector3 pos = new Vector3(GetSpawnX(true), y, 0f);
        GameObject pole = Instantiate(polePrefab, pos, Quaternion.identity, environmentRoot);

        if (addScrollDespawnToGates && pole.GetComponent<ScrollDespawn>() == null)
        {
            pole.AddComponent<ScrollDespawn>();
        }
    }

    private void SpawnEnemyBurst(int count)
    {
        float baseX = GetSpawnX(false);
        for (int i = 0; i < count; i++)
        {
            float x = baseX + i * horizontalSpacingBetweenEnemySpawns;
            Vector3 candidate = new Vector3(x, Random.Range(yMin, yMax), 0f);
            Vector3 pos = avoidOverlapOnSpawn
                ? FindClearSpawnPosition(candidate, enemySpawnClearRadius)
                : candidate;

            GameObject enemyObj = Instantiate(enemyPrefab, pos, Quaternion.identity, enemiesRoot);
            ShadowDrakeEnemy enemy = enemyObj.GetComponent<ShadowDrakeEnemy>();
            if (enemy != null && enemyShotPrefab != null)
            {
                enemy.ConfigureProjectilePrefab(enemyShotPrefab);
            }
        }
    }

    private void SpawnHeart()
    {
        Vector3 candidate = new Vector3(GetSpawnX(false), Random.Range(yMin, yMax), 0f);
        Vector3 pos = avoidOverlapOnSpawn
            ? FindClearSpawnPosition(candidate, heartSpawnClearRadius)
            : candidate;
        Instantiate(emberHeartPrefab, pos, Quaternion.identity, pickupsRoot);
    }

    private int GetCurrentEnemySpawnCount()
    {
        int step = Mathf.FloorToInt(difficultyElapsed / Mathf.Max(1f, secondsPerEnemyCountStep));
        int count = enemySpawnCountStart + step;
        return Mathf.Clamp(count, enemySpawnCountStart, enemySpawnCountMax);
    }

    private void UpdateEnemyCountUI(int count)
    {
        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemy Wave: x{count}";
        }
    }

    private Vector3 FindClearSpawnPosition(Vector3 seedPos, float radius)
    {
        Vector3 best = seedPos;
        for (int i = 0; i < Mathf.Max(1, spawnPositionRetryCount); i++)
        {
            Vector3 p = i == 0
                ? seedPos
                : new Vector3(
                    seedPos.x + Random.Range(-1.1f, 1.1f),
                    Random.Range(yMin, yMax),
                    0f);

            Collider2D[] hits = Physics2D.OverlapCircleAll(p, radius);
            bool blocked = false;
            for (int h = 0; h < hits.Length; h++)
            {
                Collider2D c = hits[h];
                if (c == null)
                {
                    continue;
                }

                // Avoid spawning on obstacles, enemies, health packs and gate score zones.
                if (c.CompareTag("Obstacle") || c.CompareTag("Enemy") || c.CompareTag("HealthPack") || c.CompareTag("GateScoreZone"))
                {
                    blocked = true;
                    break;
                }
            }

            if (!blocked)
            {
                return p;
            }

            best = p;
        }

        return best;
    }
}
