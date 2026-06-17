using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Swipe Settings")]
    [SerializeField] private float swipeThreshold = 340f;
    [SerializeField] private float rotationFactor = 0.024f;
    [SerializeField] private float maxRotation = 6f;
    [SerializeField] private float returnSpeed = 10f;
    [SerializeField] private float flyAwayDistance = 1200f;
    [SerializeField] private float flyAwayDuration = 0.25f;

    private RectTransform rectTransform;
    private Canvas canvas;

    private Vector2 startPosition;
    private Quaternion startRotation;
    private bool isAnimating = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        startPosition = rectTransform.anchoredPosition;
        startRotation = rectTransform.rotation;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        startPosition = rectTransform.anchoredPosition;
        startRotation = rectTransform.rotation;
        GameManager.Instance?.PreviewChoice(0f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        Vector2 scaledDelta = eventData.delta / canvas.scaleFactor;
        rectTransform.anchoredPosition = new Vector2(
            rectTransform.anchoredPosition.x + scaledDelta.x,
            startPosition.y);

        float xOffset = rectTransform.anchoredPosition.x - startPosition.x;
        float rotationZ = Mathf.Clamp(xOffset * rotationFactor, -maxRotation, maxRotation);
        rectTransform.rotation = Quaternion.Euler(0f, 0f, rotationZ);
        GameManager.Instance?.PreviewChoice(xOffset / swipeThreshold);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return;

        float xOffset = rectTransform.anchoredPosition.x - startPosition.x;

        if (xOffset > swipeThreshold)
        {
            StartCoroutine(FlyAway(Vector2.right));
        }
        else if (xOffset < -swipeThreshold)
        {
            StartCoroutine(FlyAway(Vector2.left));
        }
        else
        {
            StartCoroutine(ReturnToStart());
        }
    }

    private IEnumerator ReturnToStart()
    {
        isAnimating = true;

        Vector2 initialPosition = rectTransform.anchoredPosition;
        Quaternion initialRotation = rectTransform.rotation;

        float time = 0f;
        float duration = Mathf.Max(0.01f, 1f / returnSpeed);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, startPosition, t);
            rectTransform.rotation = Quaternion.Lerp(initialRotation, startRotation, t);

            yield return null;
        }

        rectTransform.anchoredPosition = startPosition;
        rectTransform.rotation = startRotation;
        GameManager.Instance?.HideChoicePreview();

        isAnimating = false;
    }

    private IEnumerator FlyAway(Vector2 direction)
    {
        isAnimating = true;

        Vector2 initialPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = initialPosition + direction * flyAwayDistance;

        float time = 0f;

        while (time < flyAwayDuration)
        {
            time += Time.deltaTime;
            float t = time / flyAwayDuration;

            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, t);

            yield return null;
        }

        if (direction == Vector2.left)
        {
            GameManager.Instance.SwipeLeft();
        }
        else if (direction == Vector2.right)
        {
            GameManager.Instance.SwipeRight();
        }

        rectTransform.anchoredPosition = startPosition;
        rectTransform.rotation = startRotation;
        GameManager.Instance?.HideChoicePreview();

        isAnimating = false;
    }
}
