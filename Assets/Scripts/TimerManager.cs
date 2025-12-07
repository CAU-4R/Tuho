using UnityEngine;
using Unity.Netcode;
using System.Collections; 

public class TimerManager : NetworkBehaviour
{
    public static TimerManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Instance = this;
    }

    public NetworkVariable<float> timeValue =
        new NetworkVariable<float>(90f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> countdown =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool isRunning = false;

    private IEnumerator StartCountdownThenTimer()
    {
        int preStart = 5;
        while (preStart > 0)
        {
            countdown.Value = preStart;
            yield return new WaitForSeconds(1f);
            preStart--;
        }

        countdown.Value = -1; // GO!
        yield return new WaitForSeconds(1f);
        countdown.Value = 0;

        isRunning = true;
    }

    void Update()
    {
        if (!IsServer || !isRunning) return;

        if (timeValue.Value > 0)
            timeValue.Value -= Time.deltaTime;
        else
        {
            timeValue.Value = 0;
            isRunning = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc()
    {
        Debug.Log("SERVER RPC RECEIVED - TIMER START");

        if (!IsServer) return;

        StartTimer();
    }

    private void StartTimer()
    {
        Debug.Log("SERVER START TIMER");

        StopAllCoroutines();
        StartCoroutine(StartCountdownThenTimer());
    }

    public void StartTimerButton()
    {
        // 항상 RPC 보내도 OK
        StartTimerServerRpc();
    }
}
