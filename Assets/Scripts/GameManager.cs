// GameManager.cs
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Prefabs")]
    public GameObject tuhoPotPrefab;
    public GameObject tuhoArrowPrefab;

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

        GameObject pot = Instantiate(potPrefab, position, rotation);

        float randomXY = Random.Range(0.18f, 0.25f); 
        float randomZ = Random.Range(0.14f, 0.22f);  

        pot.transform.localScale = new Vector3(randomXY, randomXY, randomZ);

        pot.GetComponent<NetworkObject>().Spawn();

        isPotPlaced.Value = true;
    }

    // 이 함수도 오직 서버(호스트)에서만 호출되어야 합니다.
    public void SpawnArrow(Vector3 position, Quaternion rotation, Vector3 force)
    {
        if (!IsServer) return;

        GameObject arrow = Instantiate(tuhoArrowPrefab, position, rotation);
        arrow.GetComponent<NetworkObject>().Spawn(true);
        arrow.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
    }
}