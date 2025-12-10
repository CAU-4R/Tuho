using Unity.Netcode;
using UnityEngine;

public class StartButtonController : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        // Host/Server만 표시
        if (IsServer || IsHost)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }
}
