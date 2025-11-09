using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientID;
    public int score;
    public float lifePoints;

    public PlayerData(ulong clientID, int score, float lifePoints)
    {
        this.clientID = clientID;
        this.score = score;
        this.lifePoints = lifePoints;
    }

    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID && score == other.score && lifePoints == other.lifePoints;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref lifePoints);
    }
}
