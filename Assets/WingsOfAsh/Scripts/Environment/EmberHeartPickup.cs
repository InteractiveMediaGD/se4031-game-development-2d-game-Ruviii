using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EmberHeartPickup : MonoBehaviour
{
    [SerializeField] private int healAmount = 20;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float despawnOffsetBehindPlayer = 1.5f;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            FlameHealth playerHealth = FindFirstObjectByType<FlameHealth>();
            if (playerHealth != null)
            {
                playerTransform = playerHealth.transform;
            }

            return;
        }

        if (transform.position.x < playerTransform.position.x - despawnOffsetBehindPlayer)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        FlameHealth health = other.GetComponent<FlameHealth>();
        if (health == null)
        {
            return;
        }

        health.Heal(healAmount);
        Destroy(gameObject);
    }
}
