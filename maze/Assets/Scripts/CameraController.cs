using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public GameObject ball;
    public Transform mazeParent; // Assign your maze object here
    private Rigidbody ballRb;

    [Header("Dynamic Settings")]
    public Vector3 baseOffset = new Vector3(0, 7, -7);
    public float smoothTime = 0.2f;
    public float zoomSensitivity = 0.5f;
    public float maxZoomOffset = 5f;

    private Vector3 currentVelocity = Vector3.zero;

    void Start()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        
        // If the maze isn't assigned, we'll assume the ball's parent is the maze
        if (mazeParent == null) mazeParent = ball.transform.parent;
    }

    void LateUpdate()
    {
        if (ball == null || mazeParent == null) return;

        // 1. Calculate Rotation-Aware Offset
        // This converts your static offset into "Maze Space" so the camera rotates with the maze
        Vector3 rotatedOffset = mazeParent.rotation * baseOffset;

        // 2. Dynamic Zoom based on Speed
        // As the ball moves faster, we increase the magnitude of the offset
        float speed = ballRb.linearVelocity.magnitude;
        float dynamicZoom = Mathf.Min(speed * zoomSensitivity, maxZoomOffset);
        
        // Apply the zoom in the direction of the offset (pushing the camera further away)
        Vector3 targetPosition = ball.transform.position + rotatedOffset.normalized * (rotatedOffset.magnitude + dynamicZoom);

        // 3. Smooth Movement
        // SmoothDamp is better than Lerp for cameras as it prevents "rubber-banding"
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);

        // 4. Look at the ball
        // Keeps the ball centered even when rotating
        transform.LookAt(ball.transform.position);
    }
}