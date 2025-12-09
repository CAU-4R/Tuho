using UnityEngine;
using Unity.Netcode;

public class HostOnlyUI : MonoBehaviour
{
    void Start()
    {
        // Network 초기화된 후 활성/비활성 설정
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += _ => UpdateVisibility();
            NetworkManager.Singleton.OnServerStarted += UpdateVisibility;
        }

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (NetworkManager.Singleton == null)
            return;

        // Host 또는 Server만 버튼 보임
        bool isHost = NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
        gameObject.SetActive(isHost);
    }
}
