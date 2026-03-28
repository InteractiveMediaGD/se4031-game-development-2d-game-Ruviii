using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyProjectile : MonoBehaviour
{
    [Header("Rendering")]
    [Tooltip("If empty, keeps the prefab layer. Set to your Player/Gameplay layer when shots render behind parallax.")]
    [SerializeField] private string sortingLayerName = "";
    [Tooltip("Must be above parallax / foreground backgrounds on the same Sorting Layer.")]
    [SerializeField] private int sortingOrder = 80;

    [SerializeField] private float speed = 9f;
    [SerializeField] private float lifetimeSeconds = 3f;
    [SerializeField] private int damageToPlayer = 8;
    [SerializeField] private float ignoreEnemyOverlapSeconds = 0.1f;

    private float spawnTime;

    private void Awake()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spawnTime = Time.time;
        ApplySorting();
    }

    private void ApplySorting()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                if (!string.IsNullOrEmpty(sortingLayerName))
                {
                    renderers[i].sortingLayerName = sortingLayerName;
                }

                renderers[i].sortingOrder = sortingOrder;
            }
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetimeSeconds);
    }

    private void Update()
    {
        transform.position += Vector3.left * (speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - spawnTime < ignoreEnemyOverlapSeconds && other.CompareTag("Enemy"))
        {
            return;
        }

        FlameHealth player = other.GetComponent<FlameHealth>();
        if (player == null)
        {
            player = other.GetComponentInParent<FlameHealth>();
        }

        if (player != null)
        {
            player.TakeDamage(damageToPlayer);
        }

        Destroy(gameObject);
    }
}
