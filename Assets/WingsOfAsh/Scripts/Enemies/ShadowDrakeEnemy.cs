using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShadowDrakeEnemy : MonoBehaviour
{
    [Header("Lifecycle")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float despawnOffsetBehindPlayer = 1.5f;

    [Header("Shooting")]
    [SerializeField] private GameObject enemyProjectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Vector2 fireOffset = new Vector2(-0.55f, 0f);
    [SerializeField] private Vector2 shootIntervalRange = new Vector2(1.4f, 2.6f);
    [SerializeField] private Vector2 burstShotCountRange = new Vector2(1f, 3f);
    [SerializeField] private Vector2 burstShotSpacingRange = new Vector2(0.2f, 0.45f);

    [Header("Random Up/Down Movement")]
    [SerializeField] private bool enableVerticalMovement = true;
    [SerializeField] private float verticalAmplitude = 0.85f;
    [SerializeField] private float verticalFrequency = 1.8f;
    [SerializeField] private float minY = -4.1f;
    [SerializeField] private float maxY = 4.1f;

    private ScoreManager scoreManager;
    private float nextShootTime;
    private float burstTimer;
    private int shotsLeftInBurst;
    private float startY;
    private float movePhase;

    public void ConfigureProjectilePrefab(GameObject projectilePrefab)
    {
        if (projectilePrefab != null)
        {
            enemyProjectilePrefab = projectilePrefab;
        }
    }

    private void Awake()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
        if (playerTransform == null)
        {
            FlameHealth hp = FindFirstObjectByType<FlameHealth>();
            if (hp != null)
            {
                playerTransform = hp.transform;
            }
        }

        startY = transform.position.y;
        movePhase = Random.Range(0f, Mathf.PI * 2f);
        ScheduleNextBurst();
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (transform.position.x < playerTransform.position.x - despawnOffsetBehindPlayer)
        {
            Destroy(gameObject);
        }

        HandleVerticalMovement();

        if (GamePauseManager.IsPaused)
        {
            return;
        }

        HandleShooting();
    }

    private void HandleVerticalMovement()
    {
        if (!enableVerticalMovement)
        {
            return;
        }

        float y = startY + Mathf.Sin((Time.time * verticalFrequency) + movePhase) * verticalAmplitude;
        y = Mathf.Clamp(y, minY, maxY);
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
    }

    private void HandleShooting()
    {
        if (enemyProjectilePrefab == null)
        {
            return;
        }

        if (shotsLeftInBurst > 0)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                ShootOne();
                shotsLeftInBurst--;
                float minSpacing = Mathf.Max(0.01f, burstShotSpacingRange.x);
                float maxSpacing = Mathf.Max(minSpacing, burstShotSpacingRange.y);
                burstTimer = Random.Range(minSpacing, maxSpacing);
            }

            return;
        }

        if (Time.time >= nextShootTime)
        {
            int minShots = Mathf.Max(1, Mathf.RoundToInt(burstShotCountRange.x));
            int maxShots = Mathf.Max(minShots, Mathf.RoundToInt(burstShotCountRange.y));
            shotsLeftInBurst = Random.Range(minShots, maxShots + 1);
            burstTimer = 0f;
            ScheduleNextBurst();
        }
    }

    private void ShootOne()
    {
        Vector3 spawnPos;
        if (firePoint != null)
        {
            spawnPos = firePoint.position;
        }
        else
        {
            spawnPos = transform.position + (Vector3)fireOffset;
        }

        Instantiate(enemyProjectilePrefab, spawnPos, Quaternion.identity, null);
    }

    private void ScheduleNextBurst()
    {
        float minInterval = Mathf.Max(0.1f, shootIntervalRange.x);
        float maxInterval = Mathf.Max(minInterval, shootIntervalRange.y);
        nextShootTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Projectile"))
        {
            scoreManager?.AddEnemyProjectileKillPoints();
            Destroy(other.gameObject);
            Destroy(gameObject);
            return;
        }

        if (other.GetComponent<FlameHealth>() != null)
        {
            Destroy(gameObject);
        }
    }
}
