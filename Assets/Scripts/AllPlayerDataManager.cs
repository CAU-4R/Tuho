using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AllPlayerDataManager : NetworkBehaviour
{
    public static AllPlayerDataManager Instance;

    private NetworkList<PlayerData> allPlayerData;
    private const int DEFAULT_ARROWS = 20; // 기본 화살 개수 설정

    public event Action<ulong> OnPlayerScoreChanged;
    public event Action<ulong> OnPlayerArrowChanged;

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
        allPlayerData.OnListChanged += HandleAllPlayerDataChanged;
        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;

        if (IsServer)
        {
            AddNewClientToList(NetworkManager.LocalClientId);
        }
    }

    public bool HasPlayer(ulong clientId)
    {
        foreach (var data in allPlayerData)
        {
            if (data.clientID == clientId) return true;
        }
        return false;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= AddNewClientToList;

        if (allPlayerData != null)
            allPlayerData.OnListChanged -= HandleAllPlayerDataChanged;
    }


    private void HandleAllPlayerDataChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        // 점수 또는 화살 개수가 바뀌면 UI 갱신 알림
        OnPlayerScoreChanged?.Invoke(changeEvent.Value.clientID);
        OnPlayerArrowChanged?.Invoke(changeEvent.Value.clientID);
    }

    private void AddNewClientToList(ulong clientID)
    {
        if (!IsServer) return;

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientID) return;
        }

        // 초기 화살 개수 할당
        PlayerData newPlayer = new PlayerData(clientID, 0, DEFAULT_ARROWS);
        allPlayerData.Add(newPlayer);
        PrintAllPlayerPlayerList();
    }

    private void PrintAllPlayerPlayerList()
    {
        foreach (var playerData in allPlayerData)
        {
            Debug.Log($"Player ID: {playerData.clientID}, Called by: {NetworkManager.Singleton.LocalClientId}");
        }
    }

    // 화살 개수 감소 함수 (서버 전용)
    public void DecreaseArrowCount(ulong clientId)
    {
        if (!IsServer) return;

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientId)
            {
                var currentData = allPlayerData[i];
                if (currentData.arrowCount > 0)
                {
                    PlayerData newData = new PlayerData(
                        currentData.clientID,
                        currentData.score,
                        currentData.arrowCount - 1
                    );
                    allPlayerData[i] = newData;
                }
                break;
            }
        }
    }

    // 특정 클라이언트의 남은 화살 개수 반환 (클라이언트에서 사용 가능)
    public int GetArrowCount(ulong clientId)
    {
        foreach (var data in allPlayerData)
        {
            if (data.clientID == clientId)
            {
                return data.arrowCount;
            }
        }
        return 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(ulong clientId, int amount)
    {
        IncreaseScore(clientId, amount);
    }

    public void IncreaseScore(ulong clientId, int amount)
    {
        if (!IsServer) return;

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientId)
            {
                PlayerData newData = new PlayerData(
                    allPlayerData[i].clientID,
                    allPlayerData[i].score + amount,
                    allPlayerData[i].arrowCount
                );

                allPlayerData[i] = newData;
                OnPlayerScoreChanged?.Invoke(clientId);
                break;
            }
        }
    }

    public Dictionary<ulong, int> GetAllScores()
    {
        Dictionary<ulong, int> result = new Dictionary<ulong, int>();
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            result.Add(allPlayerData[i].clientID, allPlayerData[i].score);
        }
        return result;
    }
}