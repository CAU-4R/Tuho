using Unity.Netcode;
using UnityEngine;

public class HostOnlyUI : MonoBehaviour
{
    private void Start()
    {
        // 네트워크가 없으면 숨김
        if (NetworkManager.Singleton == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Host / Client 연결 이벤트
        NetworkManager.Singleton.OnServerStarted += UpdateVisibility;
        NetworkManager.Singleton.OnClientConnectedCallback += _ => UpdateVisibility();

        // GameManager의 투호통 배치 여부를 감지
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isPotPlaced.OnValueChanged += (_, __) => UpdateVisibility();
        }

        // TimerManager의 타이머 실행 여부 감지
        if (TimerManager.Instance != null)
        {
            TimerManager.Instance.isTimerRunning.OnValueChanged += (_, __) => UpdateVisibility();
        }

        Invoke(nameof(UpdateVisibility), 0.05f);
    }

    private void UpdateVisibility()
    {
        if (NetworkManager.Singleton == null ||
            GameManager.Instance == null ||
            TimerManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        bool isHost = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        bool potPlaced = GameManager.Instance.isPotPlaced.Value;
        bool timerRunning = TimerManager.Instance.isTimerRunning.Value;

        // "투호통이 이미 배치되었고" + "타이머가 아직 시작되지 않았을 때만"
        bool shouldShow = isHost && potPlaced && !timerRunning;

        gameObject.SetActive(shouldShow);
    }
}
