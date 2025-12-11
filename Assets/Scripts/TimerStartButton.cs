using UnityEngine;
using Unity.Netcode;

public class TimerStartButton : MonoBehaviour
{
    private void OnEnable()
    {
        // Host/Server일 때만 버튼 활성화
        if (NetworkManager.Singleton != null && (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer))
        {
            gameObject.SetActive(true);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
    public void OnClickStart()
    {
        // Host일 때만 서버 RPC 실행
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        {
            TimerManager.Instance.StartTimerServerRpc();
            gameObject.SetActive(false); // 버튼 비활성화
        }
        else
        {
            Debug.Log("Client는 타이머를 직접 시작할 수 없습니다.");
        }
    }

}