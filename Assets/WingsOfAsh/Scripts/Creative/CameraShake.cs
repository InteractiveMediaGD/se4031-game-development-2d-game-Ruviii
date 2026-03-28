using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [SerializeField] private float defaultDuration = 0.18f;
    [SerializeField] private float defaultMagnitude = 0.12f;

    private CameraFollow cameraFollow;

    private void Awake()
    {
        Instance = this;
        cameraFollow = GetComponent<CameraFollow>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Shake()
    {
        Shake(defaultDuration, defaultMagnitude);
    }

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            if (cameraFollow != null)
            {
                cameraFollow.SetShakeOffset(new Vector3(x, y, 0f));
            }

            yield return null;
        }

        if (cameraFollow != null)
        {
            cameraFollow.SetShakeOffset(Vector3.zero);
        }
    }
}
