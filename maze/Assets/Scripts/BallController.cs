using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    private Rigidbody rb;
    private bool isResetting = false;

    [Header("Maze Reference")]
    public Transform mazeTransform; 

    [Header("Reset Settings")]
    public float waitTime = 0.1f;
    public float fallThreshold = -1.0f; 
    public float xRst = -8f;
    public float zRst = 5f;
    public float yRst = 2f; // Height above the maze floor

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float flexThreshold = 0.7f; 
    private bool canJump = true; 
    
    [Header("Win Text")]
    public GameObject winTextObject;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (winTextObject != null) winTextObject.SetActive(false);

        // Auto-assign if the maze is the parent of the ball
        if (mazeTransform == null && transform.parent != null)
        {
            mazeTransform = transform.parent;
        }
    }

    void Update()
    {
        HandleJump();
    }

    void LateUpdate()
    {
        if (mazeTransform == null) return;

        // Convert world position to local space to check if it's "below" the maze floor
        Vector3 localPos = mazeTransform.InverseTransformPoint(transform.position);

        if (localPos.y < fallThreshold && !isResetting)
        {
            StartCoroutine(ResetBallRoutine());
        }
    }

    private void HandleJump()
    {
        if (websocket.Instance == null) return;

        bool isFlexing = websocket.Instance.GloveFlex1 > flexThreshold;

        if (isFlexing && canJump && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            StartCoroutine(JumpCooldown());
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }

    IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(0.5f);
        canJump = true;
    }

    IEnumerator ResetBallRoutine()
    {
        isResetting = true;
        yield return new WaitForSeconds(waitTime);

        if (mazeTransform != null)
        {
            // Define the local spawn point
            Vector3 localRst = new Vector3(xRst, yRst, zRst);
            
            // Convert that local point to a World Position based on the Maze's current rotation/pos
            transform.position = mazeTransform.TransformPoint(localRst);
            
            // Reset physical forces
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        isResetting = false;
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Star")) 
        {
            other.gameObject.SetActive(false);
            if (winTextObject != null) winTextObject.SetActive(true);
        }
    }
}