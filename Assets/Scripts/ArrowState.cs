using Unity.Netcode;
using UnityEngine;

public class ArrowState : NetworkBehaviour
{
    public ulong ownerClientId;   // 누가 던졌는지 저장
    public bool hasScored = false;
}
