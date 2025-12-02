using UnityEngine;
using TMPro;

public class WindHUDController : MonoBehaviour
{
    [Header("References")]
    public Transform arrowModel;    // 회전시킬 화살표 모델 (Shaft/Head의 부모)
    public TMP_Text strengthText;   // 텍스트

    [Header("Settings")]
    public float rotationSpeed = 10f;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance == null || arrowModel == null) return;

        Vector3 windVector = GameManager.Instance.windVelocity.Value;
        float windSpeed = windVector.magnitude;

        // 1. 텍스트 업데이트
        if (strengthText != null)
        {
            if (windSpeed <= 0.01f) strengthText.text = "";
            else strengthText.text = $"{windSpeed:F1} m/s";
        }

        if (windSpeed <= 0.01f) return;

        // 2. 상대 회전 계산
        // 바람의 각도 (World)
        float windAngle = Mathf.Atan2(windVector.x, windVector.z) * Mathf.Rad2Deg;

        // 플레이어의 시선 각도 (Y축만 사용)
        float playerAngle = mainCamera.transform.eulerAngles.y;

        // 최종 화살표가 가리켜야 할 각도 = (바람 각도 - 내 시선 각도)
        float targetAngle = windAngle - playerAngle;

        // 3. 화살표 회전 적용
        // 카메라가 정면에서 보고 있으므로, 모델을 Y축으로 돌려야 좌우로 돌아감
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        arrowModel.localRotation = Quaternion.Slerp(arrowModel.localRotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}