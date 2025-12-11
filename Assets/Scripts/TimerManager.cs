using Unity.Netcode;
using UnityEngine;
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
        Instance = this;
    }

    public NetworkVariable<float> timeValue =
        new NetworkVariable<float>(15f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<int> countdown =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // ★ 클라이언트도 읽을 수 있는 NetworkVariable
    public NetworkVariable<bool> isTimerRunning =
        new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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

        isTimerRunning.Value = true;
    }

    void Update()
    {
        if (!IsServer) return;

        if (isTimerRunning.Value && timeValue.Value > 0)
        {
            timeValue.Value -= Time.deltaTime;
        }
        else if (timeValue.Value <= 0 && isTimerRunning.Value)
        {
            timeValue.Value = 0;
            isTimerRunning.Value = false;

            // OpenQuitCanvasClientRpc();

            // 시간이 다 되면 게임 종료 처리
            FinishGame();
        }
    }

    // 게임 종료 및 랭킹 산출 요청
    private void FinishGame()
    {
        if (!IsServer) return;
        Debug.Log("Game Over! Calculating Rankings...");
        if (RankingManager.Instance != null)
        {
            RankingManager.Instance.CalculateAndShowRankings();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void StartTimerServerRpc()
    {
        if (!IsServer) return;

        StopAllCoroutines();
        StartCoroutine(StartCountdownThenTimer());
    }

    [ClientRpc]
    private void OpenQuitCanvasClientRpc()
    {
        GameObject quitCanvas = GameObject.Find("QuitCanvas");

        if (quitCanvas != null)
            quitCanvas.SetActive(true);
        else
            Debug.LogWarning("QuitCanvas를 찾을 수 없습니다.");
    }
}