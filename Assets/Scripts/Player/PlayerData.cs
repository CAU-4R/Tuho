using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientID;
    public int score;
    public float lifePoints; // 체력 시스템이 없다면 삭제해도 됨

    public PlayerData(ulong clientID, int score, float lifePoints)
    {
        this.clientID = clientID;
        this.score = score;
        this.lifePoints = lifePoints;
    }

    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID &&
            score == other.score &&
            lifePoints.Equals(other.lifePoints);
}

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref lifePoints);
    }
}
