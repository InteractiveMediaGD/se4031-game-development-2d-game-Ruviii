using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool snapToTargetOnEnable = true;

    [Header("Horizontal framing (orthographic)")]
    [Tooltip("Where the player sits on screen horizontally. 0.5 = center, ~0.33 = left third (rule of thirds).")]
    [SerializeField] [Range(0.15f, 0.85f)] private float playerViewportX = 0.33f;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam == null)
        {
            _cam = Camera.main;
        }
    }

    private void OnEnable()
    {
        if (snapToTargetOnEnable)
        {
            SnapToTarget();
        }
    }

    private Vector3 GetDesiredPosition()
    {
        Vector3 t = target.position + offset;
        if (_cam == null || !_cam.orthographic)
        {
            return t;
        }

        float halfWidth = _cam.orthographicSize * _cam.aspect;
        float shift = (playerViewportX - 0.5f) * 2f * halfWidth;
        t.x -= shift;
        return t;
    }

    private void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        shakeOffset = Vector3.zero;
        transform.position = GetDesiredPosition();
    }

    private Vector3 shakeOffset;

    public void SetShakeOffset(Vector3 offset)
    {
        shakeOffset = offset;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = GetDesiredPosition();
        Vector3 posWithoutShake = transform.position - shakeOffset;
        Vector3 smoothed = Vector3.Lerp(posWithoutShake, desired, smoothSpeed * Time.deltaTime);
        transform.position = smoothed + shakeOffset;
    }
}
