using UnityEngine;

public class simpleRotate : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = new Vector3(1, 1, 1);
    public float rotSpeed = 50f;

    // Update is called once per frame
    void Update()
    {
        // Rotate takes the axis and the amount to rotate.
        // We multiply by Time.deltaTime to make it smooth and consistent.
        transform.Rotate(rotationAxis * rotSpeed * Time.deltaTime);
    }
}