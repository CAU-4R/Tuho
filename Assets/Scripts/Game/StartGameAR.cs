using System;
using Niantic.Lightship.SharedAR.Colocalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartGameAR : MonoBehaviour
{
    [Header("AR Shared Space Settings")]
    [SerializeField] private SharedSpaceManager _sharedSpaceManager;
    [SerializeField] private Texture2D _targetImage;
    [SerializeField] private float _targetImageSize = 0.2f;
    private const int MAX_CLIENTS = 2;
    private string roomName = "SharedRoom";

    [Header("UI Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private bool isHost = false;
    private bool isColocalized = false;

    public static event Action OnStartSharedSpaceHost;
    public static event Action OnJoinSharedSpaceClient;
    public static event Action OnStartGame;
    public static event Action OnStartSharedSpace;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 이벤트 등록
        _sharedSpaceManager.sharedSpaceManagerStateChanged += OnSharedSpaceManagerStateChanged;

        createButton.onClick.AddListener(OnCreateClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        startButton.onClick.AddListener(OnStartClicked);

        startButton.interactable = false;
    }

    private void OnDestroy()
    {
        _sharedSpaceManager.sharedSpaceManagerStateChanged -= OnSharedSpaceManagerStateChanged;
    }

    private void OnSharedSpaceManagerStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        if (args.Tracking)
        {
            Debug.Log("✅ Colocalization tracking success!");
            isColocalized = true;

            startButton.interactable = true;
            createButton.interactable = false;
            joinButton.interactable = false;
        }
        else
        {
            Debug.Log("⚠️ Tracking lost or not ready.");
            isColocalized = false;
        }
    }

    private void OnCreateClicked()
    {
        isHost = true;
        Debug.Log("🎮 Creating Room as Host...");
        OnStartSharedSpaceHost?.Invoke();

        TryStartSharedSpace();
    }

    private void OnJoinClicked()
    {
        isHost = false;
        Debug.Log("👥 Joining Room as Client...");
        OnJoinSharedSpaceClient?.Invoke();

        TryStartSharedSpace();
    }

    private void TryStartSharedSpace()
    {
        OnStartSharedSpace?.Invoke();

        if (_sharedSpaceManager == null)
        {
            Debug.LogError("❌ SharedSpaceManager not assigned!");
            return;
        }

        // 🔸 Mock Colocalization만 사용 (이미지 인식 X)
        var trackingOptions = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
        var roomOptions = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
            roomName,
            MAX_CLIENTS,
            "MockColocalization"
        );

        _sharedSpaceManager.StartSharedSpace(trackingOptions, roomOptions);
    }

    private void OnStartClicked()
    {
        if (!isColocalized)
        {
            Debug.LogWarning("⚠️ Cannot start game — colocalization not ready.");
            return;
        }

        Debug.Log(isHost ? "🟢 Starting as Host..." : "🔵 Starting as Client...");
        OnStartGame?.Invoke();

        if (isHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

        startButton.interactable = false;
    }
}
