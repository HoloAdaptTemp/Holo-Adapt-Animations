using System.Collections;
using UnityEngine;
using TMPro;

public class BallController : MonoBehaviour
{
    private Rigidbody rb;
    private bool isResetting = false;
    private bool gotStar = false;

    [Header("Reset Settings")]
    public Transform mazeTransform;
    public float waitTime = 0.1f;
    public float resetHeight = -2f;

    [Header("Spawn")]
    public Vector3 spawnOffset = new Vector3(0, 2f, 0);

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float flexThreshold = 0.7f; // How much you need to bend your finger (0 to 1)
    private bool canJump = true; 
    [Header("Win Text")]
    public GameObject winTextObject;
    //public TextMeshProUGUI setText;

    void Start()
    {
        // Get the Rigidbody component at the start
        rb = GetComponent<Rigidbody>();
        //transform.position = new Vector3(xPos, 2, zPos); 

        //rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    
        // Smooths out the ball's movement for the camera
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        winTextObject.SetActive(false);
        //SetText();
    }

    void Update()
    {
        HandleJump();
    }

    void LateUpdate()
    {
        // If the ball falls below the reset height and isn't already resetting
        // Convert the ball's global position into the maze's local space
         Vector3 localPos = mazeTransform.InverseTransformPoint(transform.position);

        // Check if the ball's local Y is below the threshold
        if (localPos.y < resetHeight && !isResetting)
        {
            StartCoroutine(ResetBallRoutine());
        }
    }

    void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Star")) 
        {
            other.gameObject.SetActive(false);
            gotStar = true;
            SetText();
            winTextObject.SetActive(true);
        }
    }

    private void HandleJump()
    {
        // Check if the websocket instance is available
        if (websocket.Instance == null) return;

        // Determine if the flex sensor value from the glove exceeds the threshold
        bool isFlexing = websocket.Instance.GloveFlex1 > flexThreshold;

        // Trigger jump if flexing, off cooldown, and touching the ground
        if (isFlexing && canJump && IsGrounded())
        {
            // Apply upward force to the Rigidbody
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // --- New Console Message ---
            Debug.Log("Ball Jumped! Flex Value: " + websocket.Instance.GloveFlex1);
            
            StartCoroutine(JumpCooldown());
        }
    }

    private bool IsGrounded()
    {
        // Raycast downward to check for the maze surface
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }

    IEnumerator JumpCooldown()
    {
        canJump = false;
        yield return new WaitForSeconds(0.5f); // Half-second cooldown
        canJump = true;
    }

    IEnumerator ResetBallRoutine()
    {
        isResetting = true;

        yield return new WaitForSeconds(waitTime); //

        // Reset Position to the starting height
        transform.position = mazeTransform.TransformPoint(spawnOffset);

        // Kill all momentum so it doesn't keep moving from its previous fall
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        isResetting = false;
    }
    void SetText() 
    {
       //setText.text =  "Got star: " + gotStar.ToString();
       Debug.Log("got star!" + gotStar);
    }
}