using UnityEngine;
using Unity.Netcode;
using System.Linq;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class RankingManager : NetworkBehaviour
{
    public static RankingManager Instance;

    private GameObject rankingPanel;
    private TextMeshProUGUI rankingText;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        FindUIElements();
        if (rankingPanel != null) rankingPanel.SetActive(false);
    }

    private void FindUIElements()
    {
        if (rankingPanel != null && rankingText != null) return;

        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (var canvas in allCanvases)
        {
            Transform panelTrans = canvas.transform.Find("RankingPanel");

            if (panelTrans != null)
            {
                rankingPanel = panelTrans.gameObject;

                Transform textTrans = panelTrans.Find("RankingText");
                if (textTrans != null)
                {
                    rankingText = textTrans.GetComponent<TextMeshProUGUI>();
                }
                else
                {
                    rankingText = rankingPanel.GetComponentInChildren<TextMeshProUGUI>();
                }

                return;
            }
        }

        Debug.LogWarning("모든 Canvas를 검색했으나 'RankingPanel'을 찾을 수 없습니다. Hierarchy 이름을 확인해주세요.");
    }

    public void CalculateAndShowRankings()
    {
        if (!IsServer) return;

        if (AllPlayerDataManager.Instance == null) return;

        Dictionary<ulong, int> allScores = AllPlayerDataManager.Instance.GetAllScores();

        // 점수 내림차순 정렬
        var sortedList = allScores.OrderByDescending(x => x.Value).ToList();

        ulong[] playerIds = new ulong[sortedList.Count];
        int[] scores = new int[sortedList.Count];

        for (int i = 0; i < sortedList.Count; i++)
        {
            playerIds[i] = sortedList[i].Key;
            scores[i] = sortedList[i].Value;
        }

        ShowRankingsClientRpc(playerIds, scores);
    }

    [ClientRpc]
    private void ShowRankingsClientRpc(ulong[] playerIds, int[] scores)
    {
        if (rankingPanel == null || rankingText == null) FindUIElements();

        if (rankingText != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("최종 순위");

            int currentRank = 1;

            for (int i = 0; i < playerIds.Length; i++)
            {
                // 동점 처리 로직
                if (i > 0 && scores[i] < scores[i - 1])
                {
                    currentRank = i + 1;
                }

                // 본인 확인 (노란색)
                bool isMine = (NetworkManager.Singleton.LocalClientId == playerIds[i]);
                string colorTag = isMine ? "<color=yellow>" : "<color=white>";
                string endColor = "</color>";

                sb.AppendLine($"{colorTag}{currentRank}위 : Player {playerIds[i]+1} ({scores[i]}점){endColor}");
            }
            rankingText.text = sb.ToString();
        }

        if (rankingPanel != null)
            rankingPanel.SetActive(true);
    }
}