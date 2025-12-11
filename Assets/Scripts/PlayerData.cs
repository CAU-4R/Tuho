using System;
using Unity.Netcode;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientID;
    public int score;
    public int arrowCount;

    public PlayerData(ulong clientID, int score, int arrowCount)
    {
        this.clientID = clientID;
        this.score = score;
        this.arrowCount = arrowCount;
    }

    public bool Equals(PlayerData other)
    {
        return clientID == other.clientID &&
               score == other.score &&
               arrowCount == other.arrowCount;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref score);
        serializer.SerializeValue(ref arrowCount);
    }
}
