using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(NetworkObject))]
public class PlayerController : NetworkBehaviour
{
    [Header("Throwing Parameters")]
    public float powerMultiplier = 0.01f; // 화살이 날아가는 힘 조정
    public float horizontalSensitivity = 0.001f;
    // public float verticalSensitivity = 0.001f; // Y축 드래그는 파워 계산에만 사용
    public float minThrowPower = 5f;
    public float maxThrowPower = 20f;
    public float baseUpwardAngle = 1.0f; // 포물선을 위한 고정 각도 변수

    [Header("Limit Settings")]
    public float minThrowDistance = 2.0f; // 1.5미터 이내 접근 금지

    [Header("AR Setup")]
    public LayerMask arMeshLayer; // Player 프리팹에서 할당 필수

    // 참조
    private Camera mainCamera;
    // private Transform arrowSpawnPoint;

    // 입력 관련
    private Vector2 dragStartPosition;
    private bool isDragging = false;

    // Input System 참조
    private TouchControl primaryTouch;
    private Mouse mouse;

    // 경고 UI
    private GameObject warningUI;
    private Transform potTransform; // 투호통 위치 캐싱

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        // 필수 참조 초기화
        mainCamera = Camera.main;

        // arrowSpawnPoint'를 찾는 로직 삭제
        // if (mainCamera != null) { ... }
        // if (arrowSpawnPoint == null) { ... }

        // Input System 초기화
        mouse = Mouse.current;
        primaryTouch = Touchscreen.current?.primaryTouch;

        // 오류 검사
        if (mainCamera == null) Debug.LogError("Main Camera를 찾을 수 없습니다!", this);

        // 플레이어가 생성되면 직접 UI를 찾아서 연결
        FindWarningUI();
    }

    // UI 찾는 함수
    private void FindWarningUI()
    {
        // 1. 최상위 부모인 'Canvas'를 먼저 찾습니다. (이건 켜져 있어야 찾을 수 있음)
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas != null)
        {
            // 2. 경로를 지정해서 찾습니다. (Canvas 아래의 GameCanvas 아래의 TooCloseText)
            // 중간에 있는 GameCanvas나 TooCloseText가 꺼져 있어도 잘 찾습니다.
            Transform uiTransform = canvas.transform.Find("GameCanvas/TooCloseText");

            if (uiTransform != null)
            {
                warningUI = uiTransform.gameObject;
                warningUI.SetActive(false); // 찾았으면 끄기
            }
            else
            {
                Debug.LogWarning($"[PlayerController] 'GameCanvas/TooCloseText' 경로를 찾을 수 없습니다. Hierarchy 구조와 철자를 확인하세요.");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerController] 'Canvas'를 찾을 수 없습니다.");
        }
    }

    void Update()
    {
        // 자신의 PlayerController가 아니면 아무것도 안 함
        if (!IsOwner) return;

        // 에디터/모바일 통합 입력 처리
        HandleCombinedInput();
    }

    private void HandleCombinedInput()
    {
        // 입력 소스 결정 (터치 우선)
        Vector2 inputPosition = Vector2.zero;
        bool wasPressed = false;
        bool wasReleased = false;

        if (primaryTouch != null && primaryTouch.device.enabled)
        {
            inputPosition = primaryTouch.position.ReadValue();
            wasPressed = primaryTouch.press.wasPressedThisFrame;
            wasReleased = primaryTouch.press.wasReleasedThisFrame;
        }
        else if (mouse != null && mouse.device.enabled)
        {
            inputPosition = mouse.position.ReadValue();
            wasPressed = mouse.leftButton.wasPressedThisFrame;
            wasReleased = mouse.leftButton.wasReleasedThisFrame;
        }

        // 입력 상태 처리

        // 1. 드래그 시작 (터치 또는 클릭 시작)
        if (wasPressed)
        {
            dragStartPosition = inputPosition;
            isDragging = true;
        }

        // 2. 드래그 종료 (터치 또는 클릭 뗌)
        if (wasReleased && isDragging)
        {
            isDragging = false;
            Vector2 dragEndPosition = inputPosition;

            // 디버깅 로그
            Debug.Log($"[Input] Drag Finished. PotPlaced: {GameManager.Instance.isPotPlaced.Value}, DragDist: {Vector2.Distance(dragStartPosition, dragEndPosition)}");

            if (GameManager.Instance.isPotPlaced.Value)
            {
                // 거리가 충분한지 먼저 체크하고, 통과하면 던짐
                if (CheckDistanceToPot())
                {
                    HandleArrowThrow(dragEndPosition);
                }
                else
                {
                    Debug.Log("너무 가까워서 던질 수 없습니다");
                    ShowWarningUI();
                }
            }
            else
            {
                if (Vector2.Distance(dragStartPosition, dragEndPosition) < 20.0f)
                {
                    HandlePotPlacement(inputPosition);
                }
                else
                {
                    Debug.LogWarning("투호통이 아직 배치되지 않았는데 드래그(던지기)를 시도했습니다. 탭하여 투호통을 먼저 배치하세요.");
                }
            }
        }
    }

    // 거리 체크 함수
    private bool CheckDistanceToPot()
    {
        // 1. 투호통을 아직 못 찾았다면 찾음
        if (potTransform == null)
        {
            GameObject potObj = GameObject.FindGameObjectWithTag("TuhoPot");
            if (potObj != null)
            {
                potTransform = potObj.transform;
            }
            else
            {
                // 아직 투호통이 없거나 태그 설정을 안 했다면 일단 던지게 해줌 (버그 방지)
                return true;
            }
        }

        // 2. 거리 계산 (카메라 위치 vs 투호통 위치)
        float distance = Vector3.Distance(mainCamera.transform.position, potTransform.position);

        // 3. 디버깅용 로그
        Debug.Log($"Distance to Pot: {distance:F2}m");

        // 4. 최소 거리보다 멀리 떨어져 있으면 true(던지기 가능), 아니면 false
        return distance >= minThrowDistance;
    }

    // UI 표시 함수
    private void ShowWarningUI()
    {
        // 혹시 처음에 못 찾았을 수도 있으니 다시 확인
        if (warningUI == null) FindWarningUI();

        if (warningUI != null)
        {
            warningUI.SetActive(true);
            CancelInvoke(nameof(HideWarningUI));
            Invoke(nameof(HideWarningUI), 1.5f); // 1.5초 뒤에 끄기
        }
    }

    // UI 숨기기 함수
    private void HideWarningUI()
    {
        if (warningUI != null) warningUI.SetActive(false);
    }

    private void HandlePotPlacement(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, arMeshLayer))
        {
            // Raycast 성공, 서버(호스트)에게 이 위치에 투호통을 스폰하라고 요청
            RequestPotPlacementServerRpc(hitInfo.point, Quaternion.FromToRotation(Vector3.up, hitInfo.normal));
        }
        else
        {
            // 디버깅 로그
            Debug.LogWarning("Input detected, but Raycast did not hit AR Mesh. Check Player Prefab 'Ar Mesh Layer' setting.");
        }
    }

    private void HandleArrowThrow(Vector2 dragEndPosition)
    {
        Vector2 dragVector = dragEndPosition - dragStartPosition;

        // 1. 파워 계산: 드래그의 전체 길이를 사용합니다.
        float dragLength = dragVector.magnitude;
        float throwPower = dragLength * powerMultiplier;
        // float throwPower = Mathf.Clamp(dragLength * powerMultiplier, minThrowPower, maxThrowPower);

        // 2. 방향 계산:
        // 기본 방향 = 카메라 정면 + 고정된 위쪽 방향 (포물선 보장)
        Vector3 baseDirection = (mainCamera.transform.forward + (Vector3.up * baseUpwardAngle));

        // 좌우 오프셋 = 드래그의 X값으로 좌우 방향 조절
        Vector3 directionOffset = (mainCamera.transform.right * dragVector.x * horizontalSensitivity);

        // 최종 방향
        Vector3 finalDirection = (baseDirection + directionOffset).normalized;

        // 3. 최종 힘
        Vector3 finalForce = finalDirection * throwPower;

        // 4. 초기 회전값 계산 (화살 머리가 즉시 날아갈 방향을 보도록)
        Quaternion initialRotation = Quaternion.LookRotation(finalDirection);

        // 'arrowSpawnPoint.position' 대신, mainCamera와 로컬 오프셋 값으로 월드 스폰 위치를 직접 계산합니다.

        // 로컬 오프셋 값
        Vector3 localOffset = new Vector3(0f, -0.2f, 0.5f);

        // 카메라의 현재 위치 + (카메라의 회전 * 로컬 오프셋) = 실제 월드 스폰 위치
        Vector3 spawnPosition = mainCamera.transform.position + mainCamera.transform.TransformDirection(localOffset);

        // 서버(호스트)에게 계산된 위치와 회전/힘으로 화살을 스폰하라고 요청
        RequestArrowThrowServerRpc(spawnPosition, initialRotation, finalForce);
    }

    // 클라이언트가 서버에게 투호통 배치를 요청하는 함수
    [ServerRpc]
    private void RequestPotPlacementServerRpc(Vector3 position, Quaternion rotation)
    {
        // 이 코드는 이제 서버(호스트)에서만 실행됩니다.
        // 서버는 GameManager의 스폰 함수를 안전하게 호출할 수 있습니다.
        GameManager.Instance.SpawnPot(position, rotation);
    }

    // 클라이언트가 서버에게 화살 던지기를 요청하는 함수
    [ServerRpc]
    private void RequestArrowThrowServerRpc(Vector3 position, Quaternion rotation, Vector3 force, ServerRpcParams rpcParams = default)
    {
        // rpcParams.Receive.SenderClientId를 통해 누가 요청했는지 서버는 정확히 알 수 있습니다.
        ulong shooterId = rpcParams.Receive.SenderClientId;

        // GameManager에게 쏜 사람(shooterId) 정보를 함께 전달합니다.
        GameManager.Instance.SpawnArrow(shooterId, position, rotation, force);
    }
}