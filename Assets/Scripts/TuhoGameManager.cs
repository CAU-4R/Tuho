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
        if (!IsOwner) return;    // 🔵 각 플레이어 자신의 화면에서만 입력 처리

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

            // 🔵 로컬에서 만들지 말고 서버에 요청
            SpawnPotServerRpc(hit.point, hitRotation);

            currentState = GameState.ReadyToThrow;
        }
    }

    // 🔵 서버가 Pot 생성 후 Spawn()함
    [ServerRpc]
    void SpawnPotServerRpc(Vector3 pos, Quaternion rot)
    {
        GameObject pot = Instantiate(potPrefab, pos, rot);
        pot.GetComponent<NetworkObject>().Spawn();
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

        Vector3 horizontalDir = mainCamera.transform.right * swipeVector.x + mainCamera.transform.forward * swipeVector.y;
        horizontalDir.y = 0;
        float swipeMagnitude = swipeVector.magnitude;
        Vector3 finalDirection = (horizontalDir.normalized + Vector3.up * 1.5f).normalized;

        Quaternion initialRotation = Quaternion.LookRotation(finalDirection);

        // 🔵 화살 생성도 로컬에서 직접 Instantiate 하면 안 됨
        SpawnArrowServerRpc(arrowSpawnPoint.position, initialRotation, finalDirection, swipeMagnitude, NetworkManager.Singleton.LocalClientId);
    }


    // 🔵 서버가 Arrow 생성해서 Spawn()하고 힘도 서버에서 적용
    [ServerRpc]
    void SpawnArrowServerRpc(Vector3 pos, Quaternion rot, Vector3 direction, float magnitude, ulong ownerId)
    {
        GameObject arrow = Instantiate(arrowPrefab, pos, rot);
        NetworkObject netObj = arrow.GetComponent<NetworkObject>();

        netObj.SpawnWithOwnership(ownerId);

        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        rb.AddForce(direction * magnitude * throwForceMultiplier, ForceMode.Impulse);
    }
}
