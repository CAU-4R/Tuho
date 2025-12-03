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
    while (true)
    {
        bool cond1 = NetworkManager.Singleton == null;
        bool cond2 = (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsListening);
        bool cond3 = AllPlayerDataManager.Instance == null;

        //Debug.Log($"[ScoreUI] cond1(NetworkManager null): {cond1}");
        //Debug.Log($"[ScoreUI] cond2(IsListening false): {cond2}");
        //Debug.Log($"[ScoreUI] cond3(PlayerData null): {cond3}");

        if (!cond1 && !cond2 && !cond3)
            break;

        scoreText.text = "Loading...";
        yield return new WaitForSeconds(0.5f);
    }

    // 초기화 로직 실행
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