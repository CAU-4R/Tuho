using TMPro;
using UnityEngine;

public class WindIndicator3D : MonoBehaviour
{
    [Header("Settings")]
    public float rotationSpeed = 5.0f; // 화살표가 부드럽게 돌아가는 속도

    [Header("UI References")]
    public TMP_Text strengthText; // 텍스트 컴포넌트 연결용 변수
    
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 1. 게임 매니저가 없거나 바람 데이터가 없으면 리턴
        if (GameManager.Instance == null) return;

        Vector3 windVector = GameManager.Instance.windVelocity.Value;
        float windSpeed = windVector.magnitude; // 바람의 세기 (벡터의 길이)

        // 바람이 없으면 화살표를 숨기거나 회전 중지
        if (windVector == Vector3.zero) return;

        // 2. 바람의 방향(Rotation) 계산
        // Quaternion.LookRotation: 특정 벡터 방향을 바라보는 회전값을 만들어줍니다.
        // 바람이 "부는 방향"으로 화살표가 향하게 됩니다.
        Quaternion targetRotation = Quaternion.LookRotation(windVector);

        // 3. 회전 적용
        // transform.rotation (월드 회전)을 제어합니다.
        // 부모(카메라)가 회전해도, 이 오브젝트는 바람 방향을 유지하려고 하므로
        // 결과적으로 나침반처럼 동작하게 됩니다.
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 4. 텍스트 업데이트 (소수점 1자리까지 표시)
        if (strengthText != null)
        {
            strengthText.text = $"{windSpeed:F1} m/s";

            // 5. 텍스트 빌보드 처리
            // 화살표가 돌아도, 텍스트는 항상 카메라를 정면으로 바라보게 함
            strengthText.transform.rotation = mainCamera.transform.rotation;
        }
    }
}