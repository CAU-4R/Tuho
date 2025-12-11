using Unity.Netcode;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject gameManagerPrefab;
    public GameObject playerDataPrefab; // 추가: PlayerData 프리팹 연결용 변수
    public GameObject timerManagerPrefab;
    public GameObject rankingManagerPrefab;

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

            // 3. TimerManager 생성
            if (TimerManager.Instance == null)
            {
                GameObject tm = Instantiate(timerManagerPrefab);
                tm.GetComponent<NetworkObject>().Spawn();
            }

            // 4. RankingManager 생성
            if (RankingManager.Instance == null)
            {
                GameObject rm = Instantiate(rankingManagerPrefab);
                rm.GetComponent<NetworkObject>().Spawn();
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