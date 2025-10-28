using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;

public class TuhoGameManager : MonoBehaviour
{
    // Inspector 창에서 연결할 변수들
    public GameObject potPrefab;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float throwForceMultiplier = 0.02f;

    // 내부에서 사용할 변수들
    // private ARRaycastManager raycastManager;
    private Camera mainCamera;

    private enum GameState { PlacingPot, ReadyToThrow }
    private GameState currentState = GameState.PlacingPot;

    private Vector2 startScreenPos;
    private bool isThrowing = false;

    void Awake()
    {
        // raycastManager = GetComponent<ARRaycastManager>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 포인터(마우스 또는 터치)가 없으면 아무것도 하지 않음
        if (Pointer.current == null) return;

        // 1. 투호 병 배치 상태
        if (currentState == GameState.PlacingPot)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                PlacePot();
            }
        }
        // 2. 화살 발사 준비 상태
        else if (currentState == GameState.ReadyToThrow)
        {
            HandleThrowing();
        }
    }

    /*
    void PlacePot()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();

        // Debug.Log로 어떤 표면을 감지하는지 확인합니다.
        if (raycastManager.Raycast(screenPos, hits, TrackableType.AllTypes))
        {
            // 부딪힌 첫 번째 오브젝트의 이름을 출력합니다.
            Debug.Log("Raycast Hit: " + hits[0].trackable.name);

            Pose hitPose = hits[0].pose;
            Instantiate(potPrefab, hitPose.position, hitPose.rotation);
            currentState = GameState.ReadyToThrow;
        }
        else
        {
            // 아무것도 부딪히지 않았을 때 메시지를 출력합니다.
            Debug.Log("Raycast Missed: No trackable surface found.");
        }
    }
    */

    void PlacePot()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();

        // 1. 카메라에서 화면 터치 지점으로 뻗어나가는 광선(Ray)을 만듭니다.
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // 2. 물리 엔진을 사용해 광선을 쏴서 MeshCollider와 부딪히는지 확인합니다.
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 부딪힌 지점의 표면 법선(normal)을 사용해, 표면에 맞게 오브젝트를 회전시킵니다.
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // 부딪힌 위치(hit.point)와 계산된 회전값으로 병을 생성합니다.
            Instantiate(potPrefab, hit.point, hitRotation);
            currentState = GameState.ReadyToThrow;
        }
    }

    void HandleThrowing()
    {
        // 터치를 시작했을 때
        if (Pointer.current.press.wasPressedThisFrame)
        {
            isThrowing = true;
            startScreenPos = Pointer.current.position.ReadValue();
        }

        // 터치를 떼었을 때
        if (Pointer.current.press.wasReleasedThisFrame)
        {
            // 던지기 시작한 상태에서만 발사 로직 실행
            if (isThrowing)
            {
                isThrowing = false;
                OnThrowEnd(Pointer.current.position.ReadValue());
            }
        }
    }

    void OnThrowEnd(Vector2 endScreenPos)
    {
        Vector2 swipeVector = endScreenPos - startScreenPos;

        // 1. 발사할 방향과 힘을 먼저 모두 계산합니다.
        Vector3 horizontalDir = mainCamera.transform.right * swipeVector.x + mainCamera.transform.forward * swipeVector.y;
        horizontalDir.y = 0;
        float swipeMagnitude = swipeVector.magnitude;
        Vector3 finalDirection = (horizontalDir.normalized + Vector3.up * 1.5f).normalized;

        // 2. 계산된 비행 방향(finalDirection)을 바탕으로 화살의 초기 회전값을 만듭니다.
        Quaternion initialRotation = Quaternion.LookRotation(finalDirection);

        // 3. 생성(Instantiate)하는 바로 그 순간에 올바른 위치와 '회전값'을 함께 적용합니다.
        GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, initialRotation);
        Rigidbody arrowRb = arrow.GetComponent<Rigidbody>();

        // 4. 이제 마지막으로 힘을 가합니다.
        arrowRb.AddForce(finalDirection * swipeMagnitude * throwForceMultiplier, ForceMode.Impulse);
    }

    // private List<ARRaycastHit> hits = new List<ARRaycastHit>();
}