using System.Collections;
using System.Text;
using UnityEngine;
using NativeWebSocket;

public class websocket : MonoBehaviour
{
    // --- SINGLETON SETUP ---
    public static websocket Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- PUBLIC PROPERTIES (Required by your other scripts) ---
    public Quaternion GloveRotation => latestRotation;
    public Vector3 GloveAcceleration => latestTranslation;
    public float GloveFlex1 => latestFlex1;
    public float GloveFlex2 => latestFlex2;
    public bool Button1 => latestButton1;
    public bool Button2 => latestButton2;

    // --- PRIVATE DATA ---
    private WebSocket websocketClient;
    private Quaternion latestRotation = Quaternion.identity;
    private Vector3 latestTranslation = Vector3.zero;
    private float latestFlex1 = 0f;
    private float latestFlex2 = 0f;
    private bool latestButton1 = false;
    private bool latestButton2 = false;
    private bool hasNewValues = false;

    void Start()
    {
        ConnectWebSocket();
    }

    async void ConnectWebSocket()
    {
        websocketClient = new WebSocket("ws://localhost:8765");

        websocketClient.OnOpen += () => { Debug.Log("WebSocket connection opened."); };
        websocketClient.OnError += (e) => { Debug.LogError("WebSocket error: " + e); };
        websocketClient.OnClose += (e) => { Debug.Log("WebSocket connection closed."); };

        websocketClient.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            ParseMessage(message);
        };

        await websocketClient.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocketClient?.DispatchMessageQueue();
#endif

        if (hasNewValues)
        {
            // The original behavior: This object also rotates/moves itself
            transform.rotation = latestRotation;
            transform.Translate(latestTranslation, Space.Self);
            hasNewValues = false;
        }
    }

    async void OnDestroy()
    {
        if (websocketClient != null && websocketClient.State == WebSocketState.Open)
        {
            await websocketClient.Close();
        }
    }

    void ParseMessage(string message)
    {
        string[] parts = message.Split(',');
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

            const float UNIT_MAX = 4294967295f;

            // Mapping raw values
            float pitch = (pitchRaw / UNIT_MAX) * 360f;
            float roll = (rollRaw / UNIT_MAX) * 360f;
            float yaw = (yawRaw / UNIT_MAX) * 360f;
            latestRotation = Quaternion.Euler(pitch, roll, yaw);

            float signX = accelXRaw > UNIT_MAX / 2 ? 1 : -1;
            float signY = accelYRaw > UNIT_MAX / 2 ? 1 : -1;
            float signZ = accelZRaw > UNIT_MAX / 2 ? 1 : -1;
            latestTranslation = new Vector3(
                (accelXRaw == 0 ? 0 : 0.01f * signX),
                (accelYRaw == 0 ? 0 : 0.01f * signY),
                (accelZRaw == 0 ? 0 : 0.01f * signZ)
            );

            latestFlex1 = flex1Raw / UNIT_MAX;
            latestFlex2 = flex2Raw / UNIT_MAX;
            latestButton1 = button1Raw >= UNIT_MAX / 2;
            latestButton2 = button2Raw >= UNIT_MAX / 2;

            hasNewValues = true;
        }
    }
}