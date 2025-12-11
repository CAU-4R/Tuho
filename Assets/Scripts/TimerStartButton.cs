using UnityEngine;
using Unity.Netcode;

public class TimerStartButton : MonoBehaviour
{
    public void OnClickStart()
    {
        // Host일 때만 서버 RPC 실행
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            TimerManager.Instance.StartTimerServerRpc();
        }
        else
        {
            Debug.Log("Client는 타이머를 직접 시작할 수 없습니다.");
        }
    }

}