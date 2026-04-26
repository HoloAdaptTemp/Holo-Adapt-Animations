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
    private float prevPitchRaw;
    private float prevRollRaw;
    private float prevYawRaw;

    // Bounds coming from the Python dummy generators:
    // - rotations: [-PI, PI]
    // - accel:  [-(2^31), 2^31-1]
    // - flex:   [0.0, 1.0]
    private const float ROT_MIN = 0f;
    private const float ROT_MAX = 360f;
    private const float ACCEL_MIN = -100f;
    private const float ACCEL_MAX = 100f;

    private static float Map(float value, float inMin, float inMax, float outMin, float outMax)
    {
        if (inMax - inMin == 0f) return outMin;
        return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
    }

    public Quaternion GloveRotation { get; private set; }
    public Vector3 GloveRotationEuler { get; private set; }
    public Quaternion GloveRotationDelta { get; private set; }
    public Vector3 GloveRotationDeltaEuler { get; private set; }
    public Vector3 GloveAcceleration { get; private set; }
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

            // Map rotation values (Python sends radians in [0, 360]) to degrees [0,360)
            float pitchDeg = Map(pitchRaw, ROT_MIN, ROT_MAX, 0f, 360f);
            float rollDeg = Map(rollRaw, ROT_MIN, ROT_MAX, 0f, 360f);
            float yawDeg = Map(yawRaw, ROT_MIN, ROT_MAX, 0f, 360f);

            // Compute previous mapped degrees for delta calculation
            float prevPitchDeg = Map(prevPitchRaw, ROT_MIN, ROT_MAX, 0f, 360f);
            float prevRollDeg = Map(prevRollRaw, ROT_MIN, ROT_MAX, 0f, 360f);
            float prevYawDeg = Map(prevYawRaw, ROT_MIN, ROT_MAX, 0f, 360f);


            GloveRotation = Quaternion.Euler(
                pitchDeg,
                yawDeg,
                rollDeg
            );
            GloveRotationEuler = new Vector3(pitchDeg, yawDeg, rollDeg);

            GloveRotationDelta = Quaternion.Euler(
                pitchDeg - prevPitchDeg,
                yawDeg - prevYawDeg,
                rollDeg - prevRollDeg
            );
            GloveRotationDeltaEuler = new Vector3(
                pitchDeg - prevPitchDeg,
                yawDeg - prevYawDeg,
                rollDeg - prevRollDeg
            );

            // Map rotation values (Python sends accel in [-100,100]) to [-100,100]
            float accelX = Map(accelXRaw, ACCEL_MIN, ACCEL_MAX, -100f, 100f);
            float accelY = Map(accelYRaw, ACCEL_MIN, ACCEL_MAX, -100f, 100f);
            float accelZ = Map(accelZRaw, ACCEL_MIN, ACCEL_MAX, -100f, 100f);

            GloveAcceleration = new Vector3(
                accelX,
                accelY,
                accelZ
            );

            // Flex already in [0,1] from the generator
            GloveFlex1 = Mathf.Clamp01(flex1Raw);
            GloveFlex2 = Mathf.Clamp01(flex2Raw);

            GloveButton1 = button1Raw > 0.5f;
            GloveButton2 = button2Raw > 0.5f;

            // Save current raw values for next-delta calculation
            prevPitchRaw = pitchRaw;
            prevRollRaw = rollRaw;
            prevYawRaw = yawRaw;
        }
    }
}
