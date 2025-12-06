using UnityEngine;
using TMPro;
using System.Collections;
using Unity.Netcode;

public class ArrowUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text arrowCountText;

    // 캔버스가 꺼졌다가(SetActive false) 다시 켜질 때(true)마다 실행됩니다.
    private void OnEnable()
    {
        StartCoroutine(InitArrowUI());
    }

    // 캔버스가 꺼질 때 정리
    private void OnDisable()
    {
        StopAllCoroutines(); // 안전하게 코루틴 정지
        if (AllPlayerDataManager.Instance != null)
        {
            AllPlayerDataManager.Instance.OnPlayerArrowChanged -= HandleArrowChanged;
        }
    }

    private IEnumerator InitArrowUI()
    {
        // 로딩 표시
        if (arrowCountText != null) arrowCountText.text = "-";

        // 1. 데이터 준비 대기
        while (true)
        {
            // 매니저와 네트워크 연결 확인
            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening &&
                AllPlayerDataManager.Instance != null)
            {
                // 내 데이터가 리스트에 들어왔는지 확인
                ulong myId = NetworkManager.Singleton.LocalClientId;
                if (AllPlayerDataManager.Instance.HasPlayer(myId))
                {
                    break; // 준비 완료
                }
            }
            yield return null; // 다음 프레임까지 대기
        }

        // 2. 이벤트 구독
        AllPlayerDataManager.Instance.OnPlayerArrowChanged -= HandleArrowChanged;
        AllPlayerDataManager.Instance.OnPlayerArrowChanged += HandleArrowChanged;

        // 3. 즉시 초기값 갱신
        UpdateArrowUI();
    }

    private void HandleArrowChanged(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return;

        // 내 화살 개수가 변했을 때만 갱신
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            UpdateArrowUI();
        }
    }

    private void UpdateArrowUI()
    {
        if (AllPlayerDataManager.Instance == null || NetworkManager.Singleton == null) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;
        int count = AllPlayerDataManager.Instance.GetArrowCount(myId);

        if (arrowCountText != null)
        {
            arrowCountText.text = count.ToString();
        }
    }
}