using System.Collections;
using System.Text;
using UnityEngine;
using NativeWebSocket;

public class websocket : MonoBehaviour
{
    // Singleton instance so other scripts can reference websocket.Instance
    public static websocket Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    private WebSocket _websocketClient;
    const float UNIT_MAX = 4294967295f;
    public Quaternion GloveRotation { get; private set; } // in range [0, 360] degrees for each axis
    public Vector3 GloveAcceleration { get; private set; } // in range [-100, 100]
    public float GloveFlex1 { get; private set; } // in range [0, 1]
    public float GloveFlex2 { get; private set; } // in range [0, 1]
    public bool GloveButton1 { get; private set; } // true if pressed, false otherwise
    public bool GloveButton2 { get; private set; } // true if pressed, false otherwise

    void Start()
    {
        ConnectWebSocket();
    }

    async void ConnectWebSocket()
    {
        _websocketClient = new WebSocket("ws://localhost:8765");

        _websocketClient.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened.");
        };

        _websocketClient.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
        };

        _websocketClient.OnClose += (e) =>
        {
            Debug.Log("WebSocket connection closed.");
        };

        _websocketClient.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            // Parse and apply rotation
            ParseMessage(message);
        };

        await _websocketClient.Connect();
    }

    async void OnDestroy()
    {
        if (_websocketClient != null && _websocketClient.State == WebSocketState.Open)
        {
            await _websocketClient.Close();
        }
        if (Instance == this) Instance = null;
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        _websocketClient?.DispatchMessageQueue();
#endif
    }

    void ParseMessage(string message)
    {
        string[] parts = message.Split(',');
        // CSV: pitch,roll,yaw,accel_x,accel_y,accel_z,flex_1,flex_2,button_1,button_2
        if (parts.Length >= 10)
        {
            float.TryParse(parts[0], out float pitchRaw);
            float.TryParse(parts[1], out float rollRaw);
            float.TryParse(parts[2], out float yawRaw);
            float.TryParse(parts[3], out float accelXRaw);
            float.TryParse(parts[4], out float accelYRaw);
            float.TryParse(parts[5], out float accelZRaw);
            float.TryParse(parts[6], out float flex1Raw);
            float.TryParse(parts[7], out float flex2Raw);
            float.TryParse(parts[8], out float button1Raw);
            float.TryParse(parts[9], out float button2Raw);

            GloveRotation = Quaternion.Euler(
                pitchRaw / UNIT_MAX * 360f,
                yawRaw / UNIT_MAX * 360f,
                rollRaw / UNIT_MAX * 360f
            );

            GloveAcceleration = new Vector3(
                accelXRaw / UNIT_MAX * 200f - 100f,
                accelYRaw / UNIT_MAX * 200f - 100f,
                accelZRaw / UNIT_MAX * 200f - 100f
            );

            GloveFlex1 = flex1Raw / UNIT_MAX;
            GloveFlex2 = flex2Raw / UNIT_MAX;

            GloveButton1 = button1Raw > 0.5f;
            GloveButton2 = button2Raw > 0.5f;
        }
    }
}
