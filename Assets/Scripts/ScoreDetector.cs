using UnityEngine;
using Unity.Netcode;

public class ScoreDetector : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 1. 충돌 판정은 반드시 서버에서만 수행 (중요!)
        if (!IsServer) return;

        // 2. 화살인지 확인
        // (ArrowPhysicsController나 ArrowState가 붙어있는지 확인)
        ArrowState arrow = other.GetComponent<ArrowState>();

        if (arrow != null)
        {
            // 3. 이미 점수가 계산된 화살인지 확인
            if (arrow.hasScored) return;

            arrow.hasScored = true;

            // 4. 화살의 주인 ID를 가져옴
            ulong ownerId = arrow.ownerClientId.Value;

            // 5. 점수 매니저에게 점수 추가 요청
            if (AllPlayerDataManager.Instance != null)
            {
                // 팀원 코드의 IncreaseScore 사용
                AllPlayerDataManager.Instance.IncreaseScore(ownerId, 1);
                Debug.Log($"Player {ownerId} Scored!");
            }
        }
    }
}