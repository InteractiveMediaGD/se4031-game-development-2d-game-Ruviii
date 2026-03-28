using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject projectilePrefab;

    [SerializeField] private FlameHealth flameHealth;

    [Header("Spawn")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0.6f, 0f);
    [Tooltip("Off = fire from Fire Point (or spawn offset). On = spawn at mouse (2D). Mouse mode often overlaps pickups and looks like you shoot hearts.")]
    [SerializeField] private bool spawnAtMouseWorldPosition;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        ResolveFlameHealth();
    }

    private void ResolveFlameHealth()
    {
        if (flameHealth != null)
        {
            return;
        }

        flameHealth = GetComponent<FlameHealth>();
        if (flameHealth == null)
        {
            flameHealth = GetComponentInParent<FlameHealth>();
        }
    }

    private void Update()
    {
        ResolveFlameHealth();
        if (GamePauseManager.IsPaused)
        {
            return;
        }

        if (flameHealth != null && (flameHealth.IsDead || flameHealth.IsControlLocked))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (flameHealth != null && (flameHealth.IsDead || flameHealth.IsControlLocked))
        {
            return;
        }

        if (projectilePrefab == null)
        {
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        Quaternion rot = Quaternion.identity;
        if (projectilePrefab.GetComponent<FireBreathProjectile>() == null &&
            projectilePrefab.GetComponentInChildren<FireBreathProjectile>() == null)
        {
            Debug.LogWarning(
                $"Projectile prefab '{projectilePrefab.name}' has no FireBreathProjectile. You may have assigned the EmberHeart or wrong prefab.",
                this);
        }

        ResolveFlameHealth();
        GameObject instance = Instantiate(projectilePrefab, spawnPos, rot, null);

        FireBreathProjectile projectile = instance.GetComponent<FireBreathProjectile>();
        if (projectile != null && flameHealth != null)
        {
            projectile.ApplyRageIntensity(1f - flameHealth.NormalizedHealth);
        }
    }

    private Vector3 GetSpawnPosition()
    {
        if (spawnAtMouseWorldPosition && mainCamera != null)
        {
            Vector3 screen = Input.mousePosition;
            screen.z = Mathf.Abs(mainCamera.transform.position.z);
            Vector3 world = mainCamera.ScreenToWorldPoint(screen);
            world.z = transform.position.z;
            return world;
        }

        if (firePoint != null)
        {
            return firePoint.position;
        }

        return transform.position + (Vector3)spawnOffset;
    }
}
