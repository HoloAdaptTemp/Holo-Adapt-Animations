using UnityEngine;

public class StationaryHologram : MonoBehaviour
{
    [Header("Anchor Settings")]
    public Vector3 centerPoint = Vector3.zero; // The center of your "hologram box"
    public float distance = 5f;
    public float fieldOfView = 30f;

    void Start()
    {
        SetupStationaryCameras();
    }

    void SetupStationaryCameras()
    {
        // Front
        CreateCam("Front", centerPoint + new Vector3(0, 0, -distance), 0f, new Rect(0.25f, 0f, 0.5f, 0.5f));
        // Back
        CreateCam("Back", centerPoint + new Vector3(0, 0, distance), 180f, new Rect(0.25f, 0.5f, 0.5f, 0.5f));
        // Left
        CreateCam("Left", centerPoint + new Vector3(-distance, 0, 0), 90f, new Rect(0f, 0.25f, 0.5f, 0.5f));
        // Right
        CreateCam("Right", centerPoint + new Vector3(distance, 0, 0), -90f, new Rect(0.5f, 0.25f, 0.5f, 0.5f));
    }

    void CreateCam(string label, Vector3 pos, float zRotation, Rect viewport)
    {
        GameObject camGo = new GameObject("HoloCam_" + label);
        camGo.transform.position = pos;
        
        // Point toward the centerPoint
        camGo.transform.LookAt(centerPoint);
        
        // Apply the necessary Z-roll for the pyramid reflection
        camGo.transform.Rotate(0, 0, zRotation, Space.Self);

        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.fieldOfView = fieldOfView;
        cam.rect = viewport;
    }
}