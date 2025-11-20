using UnityEngine;
using Unity.Netcode;

public class ScoreDetector : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 충돌 판정은 반드시 서버만 한다
        if (!IsServer) return;

        if (other.CompareTag("Arrow"))
        {
            ArrowState arrow = other.GetComponent<ArrowState>();

            if (arrow == null) return;
            if (arrow.hasScored) return;  // 중복 점수 방지

            arrow.hasScored = true;

            ulong playerId = arrow.ownerClientId;

            // 점수 증가
            AllPlayerDataManager.Instance.AddScoreServerRpc(playerId, 1);

            // 성공 메시지 띄우기 (서버에서 → 모든 클라이언트)
            NetworkUIManager uiManager = FindObjectOfType<NetworkUIManager>();
            if (uiManager != null)
            {
                uiManager.ShowSuccessMessageClientRpc();
            }
        }
    }
}
