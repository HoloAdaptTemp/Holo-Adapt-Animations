using UnityEngine;

public class readWebsocket : MonoBehaviour
{
    void Update()
    {
        if (websocket.Instance == null)
            return;
        Quaternion rotation = websocket.Instance.GloveRotation;
        Vector3 acceleration = websocket.Instance.GloveAcceleration;
        transform.rotation = rotation;
    }
}
