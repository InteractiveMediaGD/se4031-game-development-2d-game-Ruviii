using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FireBreathProjectile : MonoBehaviour
{
    [Header("Rendering")]
    [Tooltip("If empty, keeps the prefab layer. Set when fire renders behind parallax.")]
    [SerializeField] private string sortingLayerName = "";
    [Tooltip("Keep above background sprites on the same Sorting Layer.")]
    [SerializeField] private int sortingOrder = 80;

    [SerializeField] private float speed = 14f;
    [SerializeField] private float lifetimeSeconds = 2f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float ignoreOverlapSeconds = 0.12f;

    private float speedMultiplier = 1f;
    private float spawnTime;
    private Vector3 prefabVisualScale;

    private void Awake()
    {
        prefabVisualScale = transform.localScale;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        spawnTime = Time.time;

        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            if (srs[i] != null)
            {
                if (!string.IsNullOrEmpty(sortingLayerName))
                {
                    srs[i].sortingLayerName = sortingLayerName;
                }

                srs[i].sortingOrder = sortingOrder;
            }
        }
    }

    private void Start()
    {
        Destroy(gameObject, lifetimeSeconds);
    }

    private void Update()
    {
        transform.position += Vector3.right * (speed * speedMultiplier * Time.deltaTime);
    }

    public void ApplyRageIntensity(float lowHealth01)
    {
        lowHealth01 = Mathf.Clamp01(lowHealth01);
        speedMultiplier = Mathf.Lerp(1f, 1.3f, lowHealth01);
        float scaleMul = Mathf.Lerp(1f, 1.2f, lowHealth01);
        // Keep prefab scale (e.g. half-size vs heart); rage only multiplies on top.
        transform.localScale = prefabVisualScale * scaleMul;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(new Color(1f, 0.85f, 0.35f, 1f), new Color(1f, 0.35f, 0.1f, 1f), lowHealth01);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - spawnTime < ignoreOverlapSeconds)
        {
            if (other.CompareTag("HealthPack") || other.CompareTag("Player"))
            {
                return;
            }
        }

        // Gate gap score triggers must not eat fireballs — enemies sit behind the gate.
        if (other.GetComponentInParent<GateScoreZone>() != null)
        {
            return;
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.GetComponentInParent<GateScoreZone>() != null)
        {
            return;
        }

        Destroy(gameObject);
    }
}
