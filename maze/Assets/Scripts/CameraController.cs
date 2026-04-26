using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    public GameObject ball;
    public Transform mazeParent; 
    private Rigidbody ballRb;

    [Header("Dynamic Settings")]
    public Vector3 baseOffset = new Vector3(0, 7, -7);
    public float smoothTime = 0.2f;
    public float zoomSensitivity = 0.5f;
    public float maxZoomOffset = 5f;

    private Vector3 currentVelocity = Vector3.zero;
    
    // Mode Switch State
    private bool isDynamicMode = true;
    private bool wasButtonPressed = false;

    void Start()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        if (mazeParent == null && ball != null) mazeParent = ball.transform.parent;
    }

    void Update()
    {
        // Listen for the websocket button to toggle camera modes
        if (websocket.Instance != null)
        {
            bool buttonPressed = websocket.Instance.GloveButton1;
            if (buttonPressed && !wasButtonPressed)
            {
                isDynamicMode = !isDynamicMode;
                Debug.Log("Camera Mode Switched! Dynamic: " + isDynamicMode);
            }
            wasButtonPressed = buttonPressed;
        }
    }

    void LateUpdate()
    {
        if (ball == null || mazeParent == null) return;

        Vector3 targetPosition;

        if (isDynamicMode)
        {
            Vector3 rotatedOffset = mazeParent.rotation * baseOffset;
            float speed = ballRb != null ? ballRb.linearVelocity.magnitude : 0f;
            float dynamicZoom = Mathf.Min(speed * zoomSensitivity, maxZoomOffset);
            
            targetPosition = ball.transform.position + rotatedOffset.normalized * (rotatedOffset.magnitude + dynamicZoom);
        }
        else
        {
            // Static Mode Fallback
            targetPosition = ball.transform.position + baseOffset;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        transform.LookAt(ball.transform.position);
    }
}