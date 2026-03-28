using UnityEngine;

public class WorldScroller : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxBackgroundLayer
    {
        [Tooltip("Empty parent for decorative sprites (far sky, mid ruins, foreground). Leave unassigned to skip.")]
        public Transform root;
        [Tooltip("Scroll distance multiplier vs gameplay. 0 = static, 1 = same as gameplay, <1 slower (far), >1 faster (foreground).")]
        [Range(0f, 2f)]
        public float scrollMultiplier = 1f;
    }

    [Header("Roots To Scroll (move left each frame)")]
    [SerializeField] private Transform environmentRoot;
    [SerializeField] private Transform enemiesRoot;
    [SerializeField] private Transform pickupsRoot;

    [Header("Parallax Background (optional)")]
    [SerializeField] private ParallaxBackgroundLayer[] parallaxLayers;

    [Header("Speed")]
    [SerializeField] private float baseScrollSpeed = 3f;
    [SerializeField] private float speedIncreasePerSecond = 0.12f;
    [SerializeField] private float maxScrollSpeed = 11f;

    [Header("Boost (called by player during Space arc)")]
    [SerializeField] private float boostScrollMultiplier = 1.35f;

    private float runTime;
    private float currentBoostMultiplier = 1f;

    private void Update()
    {
        runTime += Time.deltaTime;
        float speed = Mathf.Min(baseScrollSpeed + speedIncreasePerSecond * runTime, maxScrollSpeed);
        speed *= currentBoostMultiplier;

        float dx = speed * Time.deltaTime;
        MoveRoot(environmentRoot, dx);
        MoveRoot(enemiesRoot, dx);
        MoveRoot(pickupsRoot, dx);

        if (parallaxLayers != null)
        {
            for (int i = 0; i < parallaxLayers.Length; i++)
            {
                ParallaxBackgroundLayer layer = parallaxLayers[i];
                if (layer != null && layer.root != null)
                {
                    MoveRoot(layer.root, dx * layer.scrollMultiplier);
                }
            }
        }
    }

    private static void MoveRoot(Transform root, float dx)
    {
        if (root == null)
        {
            return;
        }

        root.position += Vector3.left * dx;
    }

    public void SetBoostScrollActive(bool active)
    {
        currentBoostMultiplier = active ? boostScrollMultiplier : 1f;
    }

    public void ResetRunTime()
    {
        runTime = 0f;
    }
}
