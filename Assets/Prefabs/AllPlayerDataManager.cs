using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AllPlayerDataManager : NetworkBehaviour
{
    public static AllPlayerDataManager Instance;

    private NetworkList<PlayerData> allPlayerData;
    private const int DEFAULT_LIFE = 3;

    // 점수나 체력 변동 시 이벤트
    public event Action<ulong> OnPlayerScoreChanged;
    public event Action<ulong> OnPlayerHealthChanged;

    // 🔹 플레이어 사망 이벤트 추가
    public event Action<ulong> OnPlayerDead;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        allPlayerData = new NetworkList<PlayerData>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            AddNewClientToList(NetworkManager.LocalClientId);
        }

        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= AddNewClientToList;
    }

    // 클라이언트가 연결될 때 호출
    private void AddNewClientToList(ulong clientID)
    {
        if (!IsServer) return;

        // NetworkList에 중복 체크 (Any 대신 for 루프)
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientID)
                return;
        }

        PlayerData newPlayer = new PlayerData(clientID, 0, DEFAULT_LIFE);
        allPlayerData.Add(newPlayer);

        Debug.Log($"✅ Player {clientID} joined. Total players: {allPlayerData.Count}");
        PrintAllPlayers();
    }

    // 점수 추가
    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(ulong clientID, int amount)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientID)
            {
                PlayerData updated = allPlayerData[i];
                updated.score += amount;
                allPlayerData[i] = updated;

                Debug.Log($"🏹 Player {clientID} scored! New score: {updated.score}");
                OnPlayerScoreChanged?.Invoke(clientID);
                break;
            }
        }
    }

    // 예: 체력 감소 메서드
[ServerRpc(RequireOwnership = false)]
public void DecreaseLifeServerRpc(ulong clientID, int amount)
{
    for (int i = 0; i < allPlayerData.Count; i++)
    {
        if (allPlayerData[i].clientID == clientID)
        {
            PlayerData updated = allPlayerData[i];
            updated.lifePoints -= amount;

            allPlayerData[i] = updated;

            OnPlayerHealthChanged?.Invoke(clientID);

            // 체력이 0 이하이면 사망 처리
            if (updated.lifePoints <= 0)
            {
                Debug.Log($"💀 Player {clientID} is dead!");
                OnPlayerDead?.Invoke(clientID);
            }

            break;
        }
    }
}


    // 특정 플레이어 점수 조회
    public int GetScore(ulong clientID)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientID)
                return allPlayerData[i].score;
        }
        return 0;
    }

    // 모든 플레이어 점수 조회
    public Dictionary<ulong, int> GetAllScores()
    {
        Dictionary<ulong, int> result = new Dictionary<ulong, int>();
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            result.Add(allPlayerData[i].clientID, allPlayerData[i].score);
        }
        return result;
    }

    // 디버그용
    private void PrintAllPlayers()
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            Debug.Log($"Player {allPlayerData[i].clientID}: Score={allPlayerData[i].score}, Life={allPlayerData[i].lifePoints}");
        }
    }
}
