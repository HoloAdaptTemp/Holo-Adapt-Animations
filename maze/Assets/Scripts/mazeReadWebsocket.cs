using UnityEngine;

public class mazeReadWebsocket : MonoBehaviour
{
    [Header("Rotation Limits")]
    public float maxAngle = 30f;
    public float softLimitThreshold = 0.8f;

    [Header("Smoothing")]
    public float lerpSpeed = 10f;
    [Range(0, 1)]
    public float inputSmoothSpeed = 0.15f; // Lower = smoother but more lag

    private Quaternion smoothedGloveRotation;

    void Start()
    {
        if (websocket.Instance != null)
        {
            smoothedGloveRotation = websocket.Instance.GloveRotation;
        }
    }

    void Update()
    {
        if (websocket.Instance == null) return;

        smoothedGloveRotation = Quaternion.Slerp(
            smoothedGloveRotation, 
            websocket.Instance.GloveRotation, 
            inputSmoothSpeed
        );

        Vector3 euler = smoothedGloveRotation.eulerAngles;

        float clampedX = SoftClamp(NormalizeAngle(euler.x), maxAngle);
        float clampedZ = SoftClamp(NormalizeAngle(euler.z), maxAngle);

        Quaternion targetRotation = Quaternion.Euler(clampedX, euler.y, clampedZ);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, 
            targetRotation, 
            Time.deltaTime * lerpSpeed
        );
    }

    float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    float SoftClamp(float angle, float limit)
    {
        float absAngle = Mathf.Abs(angle);
        float threshold = limit * softLimitThreshold;

        if (absAngle <= threshold) return angle;

        float overLimit = absAngle - threshold;
        float maxOver = limit - threshold;
        
        float softAngle = threshold + (maxOver * Mathf.Atan(overLimit / maxOver) / (Mathf.PI / 2f));
        
        return Mathf.Sign(angle) * softAngle;
    }
}