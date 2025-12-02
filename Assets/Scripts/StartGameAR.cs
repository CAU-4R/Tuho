using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartGameAR : MonoBehaviour

{
    private const int MAX_AMOUNT_CLIENTS_ROOM = 2;

    [SerializeField] private Button StartGameButton;
    [SerializeField] private Button CreateRoomButton;
    [SerializeField] private Button JoinRoomButton;

    private bool isHost;

    public static event Action OnStartSharedSpaceHost;
    public static event Action OnJoinSharedSpaceClient;
    public static event Action OnEnterGameCanvas; 
    public static event Action OnStartGame;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        StartGameButton.onClick.AddListener(StartGame);
        CreateRoomButton.onClick.AddListener(CreateGameHost);
        JoinRoomButton.onClick.AddListener(JoinGameClient);
    }

    void StartGame()
    {
        Debug.Log("Start Game pressed");
        OnStartGame?.Invoke();   // 타이머 시작 이벤트
    }

    void CreateGameHost()
    {
        isHost = true;
        Debug.Log("Host selected");
        OnStartSharedSpaceHost?.Invoke();
        NetworkManager.Singleton.StartHost();

        OnEnterGameCanvas?.Invoke();
    }

    void JoinGameClient()
    {
        isHost = false;
        Debug.Log("Client selected");
        OnJoinSharedSpaceClient?.Invoke();
        NetworkManager.Singleton.StartClient();

        OnEnterGameCanvas?.Invoke();
    }
}
