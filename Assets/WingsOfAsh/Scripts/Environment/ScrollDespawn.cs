using UnityEngine;

public class ScrollDespawn : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float despawnBehindPlayer = 12f;

    private void Awake()
    {
        if (playerTransform == null)
        {
            FlameHealth hp = FindFirstObjectByType<FlameHealth>();
            if (hp != null)
            {
                playerTransform = hp.transform;
            }
        }
    }

    private void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        if (transform.position.x < playerTransform.position.x - despawnBehindPlayer)
        {
            Destroy(gameObject);
        }
    }
}
