using UnityEngine;

/// <summary>
/// Unity's <see cref="UnityEngine.UI.Slider"/> drives the handle <see cref="RectTransform"/> anchors
/// every frame (horizontal: x = normalized value, y = 0..1 stretch). That vertical stretch is why a
/// coin on the handle looks squashed unless you use Preserve Aspect — or put the artwork on a child
/// that stays centered with a fixed square size. This script keeps that child fixed.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class SliderHandleIconLayout : MonoBehaviour
{
    [Tooltip("Usually a child with the coin Image. Drag here, or leave empty to use the first child.")]
    [SerializeField] private RectTransform iconGraphic;

    [SerializeField] private float iconSize = 40f;

    private void Awake()
    {
        if (iconGraphic == null && transform.childCount > 0)
        {
            iconGraphic = transform.GetChild(0) as RectTransform;
        }
    }

    private void LateUpdate()
    {
        if (iconGraphic == null)
        {
            return;
        }

        iconGraphic.anchorMin = new Vector2(0.5f, 0.5f);
        iconGraphic.anchorMax = new Vector2(0.5f, 0.5f);
        iconGraphic.pivot = new Vector2(0.5f, 0.5f);
        iconGraphic.sizeDelta = new Vector2(iconSize, iconSize);
        iconGraphic.anchoredPosition = Vector2.zero;
        iconGraphic.localScale = Vector3.one;
    }
}
