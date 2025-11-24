using Unity.Netcode;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject gameManagerPrefab;
    public GameObject playerDataPrefab; // 🔥 새로 추가: PlayerData 프리팹 연결용 변수

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            // 1. GameManager 생성
            if (GameManager.Instance == null)
            {
                GameObject go = Instantiate(gameManagerPrefab);
                go.GetComponent<NetworkObject>().Spawn();
            }

            // 2. AllPlayerDataManager 생성 (추가된 부분)
            if (AllPlayerDataManager.Instance == null)
            {
                GameObject pd = Instantiate(playerDataPrefab);
                pd.GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }
}