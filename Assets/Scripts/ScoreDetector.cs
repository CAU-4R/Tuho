// using UnityEngine;

// public class ScoreDetector : MonoBehaviour
// {
//     private bool hasScored = false;

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Arrow"))
//         {
//             ArrowState arrow = other.GetComponent<ArrowState>();
//             if (arrow != null && !arrow.hasScored)
//             {
//                 arrow.hasScored = true;

//                 // 서버에 점수 올리기 요청
//                 if (AllPlayerDataManager.Instance != null)
//                 {
//                     // arrow.ownerClientId 같은 ID를 ArrowState에서 가지고 있다고 가정
//                     ulong playerId = arrow.ownerClientId;
//                     AllPlayerDataManager.Instance.AddScoreServerRpc(playerId, 1);
//                 }

//                 // 성공 메시지는 NetworkUIManager에서 이벤트로 처리됨
//             }
//         }
//     }
// }
