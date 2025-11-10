using UnityEngine;

public class BubbleEffect : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float floatSpeed = 0.8f;
    public float floatAmplitude = 0.3f;
    public float lifetime = 1.5f;
    public Vector3 startScale = Vector3.one * 0.05f;
    public Vector3 endScale = Vector3.one * 0.15f;

    [Header("Smooth Movement")]
    public float smoothness = 2f;
    public float rotationSpeed = 15f;

    private Vector3 startPosition;
    private float startTime;
    private Vector3 currentVelocity;
    private Vector3 targetPosition;

    void Start()
    {
        startPosition = transform.position;
        startTime = Time.time;
        targetPosition = startPosition;

        transform.localScale = startScale;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Color bubbleColor = new Color(1f, 1f, 1f, 0.7f);
            renderer.material.color = bubbleColor;
        }

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        float elapsedTime = Time.time - startTime;
        float lifeRatio = elapsedTime / lifetime;

        targetPosition = startPosition + Vector3.up * elapsedTime * floatSpeed;

        targetPosition.x += Mathf.Sin(elapsedTime * 1.5f) * floatAmplitude * 0.05f;
        targetPosition.z += Mathf.Cos(elapsedTime * 1.2f) * floatAmplitude * 0.05f;

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothness * Time.deltaTime);

        transform.localScale = Vector3.Lerp(startScale, endScale, lifeRatio);

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

        if (lifeRatio > 0.7f)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                Color currentColor = renderer.material.color;
                currentColor.a = Mathf.Lerp(0.7f, 0f, (lifeRatio - 0.7f) / 0.3f);
                renderer.material.color = currentColor;
            }
        }
    }
}