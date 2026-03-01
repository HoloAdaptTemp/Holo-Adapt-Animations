using UnityEngine;

public class ReadWebsocket : MonoBehaviour
{
    void Update()
    {
        if (websocket.Instance == null)
            return;

        Quaternion gloveRotation = websocket.Instance.GloveRotation;

        Vector3 euler = gloveRotation.eulerAngles;

        transform.rotation = Quaternion.Euler(euler.x, euler.y, 0f - 90 + euler.z);
    }
}
