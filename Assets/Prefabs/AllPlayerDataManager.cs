using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AllPlayerDataManager : NetworkBehaviour
{
    public static AllPlayerDataManager Instance;

    private NetworkList<PlayerData> allPlayerData;
    private const int DEFAULT_LIFE = 3;

    public event Action<ulong> OnPlayerScoreChanged;
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

    private void AddNewClientToList(ulong clientID)
    {
        if (!IsServer) return;

        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == clientID) return;
        }

        PlayerData newPlayer = new PlayerData(clientID, 0, DEFAULT_LIFE);
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
                    allPlayerData[i].lifePoints
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
