using Unity.Netcode;
using UnityEngine;

public class ArrowState : NetworkBehaviour
{
    // 누가 던졌는지 식별하기 위한 ID (기본값 설정)
    public NetworkVariable<ulong> ownerClientId = new NetworkVariable<ulong>();

    public bool hasScored = false; // 로컬에서 단순 체크용으로 유지하거나 제거 가능
}