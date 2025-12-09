using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Prefabs")]
    public GameObject tuhoPotPrefab;
    public GameObject tuhoArrowPrefab;

    [Header("Wind Settings")]
    public float maxWindStrength = 5.0f; // 바람의 최대 세기

    // 바람 벡터 동기화 (초기값 (0,0,0), 모두 읽기 가능, 서버만 쓰기 가능)
    public NetworkVariable<Vector3> windVelocity = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // 투호통 배치 상태 (모두가 읽고, 서버만 쓴다)
    public NetworkVariable<bool> isPotPlaced = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        // Instance가 존재하지만, Unity 오브젝트로서 파괴된 상태(null)라면 새로운 주인으로 덮어씌움
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // 만약 씬 전환 시에도 유지되어야 한다면 아래 주석 해제
        // DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        // 추가: 서버가 시작될 때 랜덤 바람 설정
        if (IsServer)
        {
            // X, Z 축으로만 바람이 불도록 설정 (Y축은 보통 중력만 작용하도록 0으로 둠, 필요시 변경 가능)
            float windX = Random.Range(-1f, 1f);
            float windZ = Random.Range(-1f, 1f);

            // 정규화 후 랜덤 세기 곱하기
            Vector3 randomWind = new Vector3(windX, 0, windZ).normalized * Random.Range(1.0f, maxWindStrength);

            windVelocity.Value = randomWind;

            Debug.Log($"[GameManager] Wind Generated: {windVelocity.Value}");
        }
    }

    // 오브젝트가 파괴될 때 Instance 참조 정리
    public override void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        base.OnDestroy();
    }

    // 이 함수는 오직 서버(호스트)에서만 호출되어야 합니다.
    // PlayerController의 ServerRpc 내부에서 호출될 것입니다.
    public void SpawnPot(Vector3 position, Quaternion rotation)
    {
        // 서버가 아니면 즉시 종료
        if (!IsServer) return;

        if (isPotPlaced.Value) return;

        GameObject spawnedPot = Instantiate(tuhoPotPrefab, position, rotation);
        spawnedPot.GetComponent<NetworkObject>().Spawn(true);
        isPotPlaced.Value = true;
    }

    // 이 함수도 오직 서버(호스트)에서만 호출되어야 합니다.
    // 매개변수에 clientId 추가
    public void SpawnArrow(ulong clientId, Vector3 position, Quaternion rotation, Vector3 force)
    {
        if (!IsServer) return;

        GameObject arrow = Instantiate(tuhoArrowPrefab, position, rotation);
        NetworkObject netObj = arrow.GetComponent<NetworkObject>();
        netObj.Spawn(true);

        // 화살 주인 ID 설정
        ArrowState state = arrow.GetComponent<ArrowState>();
        if (state != null)
        {
            state.ownerClientId.Value = clientId;
        }

        arrow.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }
}