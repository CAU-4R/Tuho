using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem;
using Unity.Netcode;

public class TuhoGameManager : NetworkBehaviour
{
    public GameObject potPrefab;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float throwForceMultiplier = 0.02f;

    private Camera mainCamera;

    private enum GameState { PlacingPot, ReadyToThrow }
    private GameState currentState = GameState.PlacingPot;

    private Vector2 startScreenPos;
    private bool isThrowing = false;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // 🔥 기존의 "if (!IsOwner) return" 제거 — 모든 플레이어가 입력 가능해야 함
        if (Pointer.current == null) return;

        if (currentState == GameState.PlacingPot)
        {
            if (Pointer.current.press.wasPressedThisFrame)
            {
                PlacePot();
            }
        }
        else if (currentState == GameState.ReadyToThrow)
        {
            HandleThrowing();
        }
    }

    void PlacePot()
    {
        Vector2 screenPos = Pointer.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            // 🔥 Pot 은 소유권 필요 없음 → 서버가 생성 후 모든 클라이언트에 동기화
            SpawnPotServerRpc(hit.point, hitRotation);

            currentState = GameState.ReadyToThrow;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnPotServerRpc(Vector3 pos, Quaternion rot)
    {
        GameObject pot = Instantiate(potPrefab, pos, rot);
        pot.GetComponent<NetworkObject>().Spawn(); // 🔥 그냥 Spawn() — 서버 소유
    }

    void HandleThrowing()
    {
        if (Pointer.current.press.wasPressedThisFrame)
        {
            isThrowing = true;
            startScreenPos = Pointer.current.position.ReadValue();
        }

        if (Pointer.current.press.wasReleasedThisFrame)
        {
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

        Vector3 horizontalDir =
            mainCamera.transform.right * swipeVector.x +
            mainCamera.transform.forward * swipeVector.y;

        horizontalDir.y = 0;
        float swipeMagnitude = swipeVector.magnitude;

        Vector3 finalDirection = (horizontalDir.normalized + Vector3.up * 1.5f).normalized;
        Quaternion initialRotation = Quaternion.LookRotation(finalDirection);

        // 🔥 모든 클라이언트가 Arrow 생성 요청 가능
        SpawnArrowServerRpc(
            arrowSpawnPoint.position,
            initialRotation,
            finalDirection,
            swipeMagnitude
        );
    }

    [ServerRpc(RequireOwnership = false)]
    void SpawnArrowServerRpc(
        Vector3 pos,
        Quaternion rot,
        Vector3 direction,
        float magnitude,
        ServerRpcParams rpcParams = default
    )
    {
        // 🔥 서버가 화살 생성
        GameObject arrow = Instantiate(arrowPrefab, pos, rot);

        NetworkObject netObj = arrow.GetComponent<NetworkObject>();

        // 🔥 SpawnWithOwnership() 금지
        //    → 사용하면 owner 클라이언트 외에는 안 보이는 버그 발생함
        netObj.Spawn();  // 서버 소유, 전 클라이언트 동기화

        // 🔥 서버에서 물리 힘 적용 → 모든 클라이언트 동일하게 반영
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.AddForce(direction * magnitude * throwForceMultiplier, ForceMode.Impulse);
    }
}
