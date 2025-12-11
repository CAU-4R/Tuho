using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private GameObject noArrowUI;
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
    // UI 찾는 함수
    private void FindWarningUI()
    {
        GameObject canvas = GameObject.Find("Canvas");

        if (canvas != null)
        {
            // 1. 거리 경고 UI 찾기
            Transform uiTransform = canvas.transform.Find("GameCanvas/TooCloseText");
            if (uiTransform != null)
            {
                warningUI = uiTransform.gameObject;
                warningUI.SetActive(false);
            }

            // 2. 화살 소진 UI 찾기
            Transform noArrowTransform = canvas.transform.Find("GameCanvas/NoArrowText");
            if (noArrowTransform != null)
            {
                noArrowUI = noArrowTransform.gameObject;
                noArrowUI.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[PlayerController] 'GameCanvas/NoArrowText' 경로를 찾을 수 없습니다.");
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

    // [수정됨] UI 감지 함수 강화
    private bool IsPointerOverUIObject(Vector2 touchPos)
    {
        // 1. EventSystem 자체가 없으면 감지 불가
        if (EventSystem.current == null) return false;

        // 2. [가장 중요] 이미 EventSystem이 이번 프레임에 UI 상호작용(클릭 등)을 처리했는지 확인
        // 버튼을 누르자마자 버튼이 사라지는 경우에도, 이 값은 true로 남아있을 확률이 높습니다.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            Debug.Log("Blocked by EventSystem.current.IsPointerOverGameObject()");
            return true;
        }

        // 터치 입력의 경우 ID로도 확인 (모바일 대응)
        if (primaryTouch != null && primaryTouch.device.enabled && primaryTouch.press.isPressed)
        {
            // 터치 ID로 확인하는 로직은 Input System에서 까다로울 수 있으므로
            // 아래의 수동 Raycast가 그 역할을 대신합니다.
        }

        // 3. 수동 Raycast (UI가 살아있는 경우 물리적 위치 체크)
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = touchPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

        // 디버깅용: 무엇이 감지되었는지 확인하고 싶다면 주석 해제
        // if (results.Count > 0) Debug.Log($"Blocked by UI Raycast: {results[0].gameObject.name}");

        return results.Count > 0;
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
            // [수정] UI 위에서 눌렀다면 드래그 시작 자체를 막음
            if (IsPointerOverUIObject(inputPosition))
            {
                isDragging = false;
                Debug.Log("Input Ignored: Touched UI");
                return;
            }

            dragStartPosition = inputPosition;
            isDragging = true;
        }

        // 2. 드래그 종료 (터치 또는 클릭 뗌)
        if (wasReleased && isDragging)
        {
            if (IsPointerOverUIObject(inputPosition))
            {
                isDragging = false;
                return;
            }

            isDragging = false;
            Vector2 dragEndPosition = inputPosition;

            // 디버깅 로그
            Debug.Log($"[Input] Drag Finished. PotPlaced: {GameManager.Instance.isPotPlaced.Value}, DragDist: {Vector2.Distance(dragStartPosition, dragEndPosition)}");

            if (GameManager.Instance.isPotPlaced.Value)
            {
                int myArrows = AllPlayerDataManager.Instance.GetArrowCount(OwnerClientId);

                if (myArrows <= 0)
                {
                    Debug.Log("화살이 모두 소진되었습니다.");
                    ShowNoArrowUI();
                }
                else
                {
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
            }
            else
            {
                if (Vector2.Distance(dragStartPosition, dragEndPosition) < 20.0f)
                {
                    // 호스트만 배치 가능
                    if (IsServer)
                    {
                        HandlePotPlacement(inputPosition);
                    }
                    else
                    {
                        Debug.Log("투호통은 호스트만 배치할 수 있습니다.");
                    }
                }
                else
                {
                    Debug.LogWarning("투호통이 배치되지 않았습니다. 탭하여 배치하세요.");
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

    // 거리 경고 UI 표시 함수
    private void ShowWarningUI()
    {
        if (warningUI == null) FindWarningUI();

        if (warningUI != null)
        {
            warningUI.SetActive(true);
            CancelInvoke(nameof(HideWarningUI));
            Invoke(nameof(HideWarningUI), 1.5f); // 1.5초 뒤에 끄기
        }
    }

    // 거리 경고 UI 숨기기 함수
    private void HideWarningUI()
    {
        if (warningUI != null) warningUI.SetActive(false);
    }

    // 화살 없음 UI 표시 함수
    private void ShowNoArrowUI()
    {
        if (noArrowUI == null) FindWarningUI();

        if (noArrowUI != null)
        {
            noArrowUI.SetActive(true);
            CancelInvoke(nameof(HideNoArrowUI));
            Invoke(nameof(HideNoArrowUI), 1.5f);
        }
    }

    // 화살 없음 UI 숨기기 함수
    private void HideNoArrowUI()
    {
        if (noArrowUI != null) noArrowUI.SetActive(false);
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
        float forwardSign = Mathf.Sign(dragVector.y);
        Vector3 baseDirection = (mainCamera.transform.forward * forwardSign) + (Vector3.up * baseUpwardAngle);

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
        if (!IsServer) return;
        GameManager.Instance.SpawnPot(position, rotation);
    }

    // 클라이언트가 서버에게 화살 던지기를 요청하는 함수
    [ServerRpc]
    private void RequestArrowThrowServerRpc(Vector3 position, Quaternion rotation, Vector3 force, ServerRpcParams rpcParams = default)
    {
        // rpcParams.Receive.SenderClientId를 통해 누가 요청했는지 서버는 정확히 알 수 있습니다.
        ulong shooterId = rpcParams.Receive.SenderClientId;

        // 서버에서 화살 개수 차감 요청
        AllPlayerDataManager.Instance.DecreaseArrowCount(shooterId);

        // GameManager에게 쏜 사람(shooterId) 정보를 함께 전달합니다.
        GameManager.Instance.SpawnArrow(shooterId, position, rotation, force);
    }
}