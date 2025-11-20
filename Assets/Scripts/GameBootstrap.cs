using Unity.Netcode;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject gameManagerPrefab; // 여기에 GameManager 프리팹을 연결

    void Start()
    {
        // NetworkManager의 이벤트에 구독합니다.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        // 이 코드는 오직 서버(호스트)에서만 실행됩니다.
        // 서버가 시작되면 GameManager를 생성하고 네트워크에 스폰합니다.
        if (NetworkManager.Singleton.IsServer)
        {
            // 중복 방지: 이미 있는지 확인
            if (GameManager.Instance == null)
            {
                GameObject go = Instantiate(gameManagerPrefab);
                // 네트워크 객체로 스폰하여 모든 클라이언트에게 복제
                go.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    void OnDestroy()
    {
        // 오브젝트가 파괴될 때 이벤트 구독을 해제합니다.
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}