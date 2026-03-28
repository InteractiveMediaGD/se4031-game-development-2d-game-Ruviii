using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerFlightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlameHealth flameHealth;

    [Header("Flappy Control")]
    [SerializeField] private float flapVelocity = 6.5f;
    [SerializeField] private float maxFallVelocity = -10.5f;
    [SerializeField] private float gravityScale = 2f;

    [Header("Bounds")]
    [SerializeField] private float fixedX = -6f;
    [SerializeField] private float minY = -4.25f;
    [SerializeField] private float maxY = 4.25f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = gravityScale;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        fixedX = transform.position.x;

        if (flameHealth == null)
        {
            flameHealth = GetComponent<FlameHealth>();
            if (flameHealth == null)
            {
                flameHealth = GetComponentInParent<FlameHealth>();
            }
        }
    }

    private bool IsPlayerDead()
    {
        return flameHealth != null && (flameHealth.IsDead || flameHealth.IsControlLocked);
    }

    private void Update()
    {
        if (GamePauseManager.IsPaused)
        {
            return;
        }

        if (IsPlayerDead())
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.y = flapVelocity;
            rb.linearVelocity = velocity;
        }
    }

    private void FixedUpdate()
    {
        if (GamePauseManager.IsPaused || IsPlayerDead())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        if (velocity.y < maxFallVelocity)
        {
            velocity.y = maxFallVelocity;
            rb.linearVelocity = velocity;
        }

        ClampToBounds();
    }

    private void ClampToBounds()
    {
        Vector3 pos = transform.position;
        pos.x = fixedX;
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        Vector2 velocity = rb.linearVelocity;
        if ((pos.y <= minY && velocity.y < 0f) || (pos.y >= maxY && velocity.y > 0f))
        {
            velocity.y = 0f;
            rb.linearVelocity = velocity;
        }
    }

}
