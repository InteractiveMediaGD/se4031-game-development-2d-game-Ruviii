using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GateScoreZone : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;

    private bool scored;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (scored)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        if (scoreManager == null)
        {
            return;
        }

        scoreManager.AddGatePoint();
        scored = true;
    }
}
