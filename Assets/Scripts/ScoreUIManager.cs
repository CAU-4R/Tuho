using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections;
// using Unity.Netcode; // NetworkBehaviour를 안 쓰므로 없어도 되지만, NetworkManager 참조용으로 남겨둠
using Unity.Netcode;

// 1. NetworkBehaviour -> MonoBehaviour로 변경
public class ScoreUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    // 2. OnNetworkSpawn 대신 Start 사용
    private void Start()
    {
        StartCoroutine(InitScoreUI());
    }

    private IEnumerator InitScoreUI()
    {
        // 네트워크 매니저나 데이터 매니저가 준비될 때까지 대기
        // (접속 전이라도 UI는 살아있으므로, 접속할 때까지 기다려야 함)
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening || AllPlayerDataManager.Instance == null)
        {
            // 접속 대기 문구 표시 (선택 사항)
            if (scoreText != null) scoreText.text = "Loading...";
            yield return new WaitForSeconds(0.5f);
        }

        // 3. 이벤트 구독 및 초기화
        AllPlayerDataManager.Instance.OnPlayerScoreChanged += HandleScoreChanged;
        UpdateAllScores();
    }

    private void OnDestroy()
    {
        // 오브젝트 파괴 시 안전하게 구독 해제
        if (AllPlayerDataManager.Instance != null)
        {
            AllPlayerDataManager.Instance.OnPlayerScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(ulong clientId)
    {
        UpdateAllScores();
    }

    private void UpdateAllScores()
    {
        if (AllPlayerDataManager.Instance == null) return;

        var scores = AllPlayerDataManager.Instance.GetAllScores();
        if (scores == null || scores.Count == 0)
        {
            scoreText.text = "Waiting for players...";
            return;
        }

        // UI 표시 로직 (기존과 동일)
        string finalString = "";
        var sortedScores = scores.OrderBy(x => x.Key);

        foreach (var item in sortedScores)
        {
            string myMark = (NetworkManager.Singleton != null && item.Key == NetworkManager.Singleton.LocalClientId) ? " (Me)" : "";
            finalString += $"P{item.Key}{myMark}: {item.Value} pts\n";
        }

        scoreText.text = finalString;
    }
}