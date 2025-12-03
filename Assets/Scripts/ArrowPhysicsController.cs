using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArrowPhysicsController : MonoBehaviour
{
    private Rigidbody rb;
    private ArrowStabilizer stabilizer; // ArrowStabilizer를 제어하기 위한 변수

    private float sleepTimer = 0f;
    private const float timeUntilSleep = 2f;
    private bool hasCollided = false; // 충돌 여부를 확인하는 깃발

    // 추가: 바람 영향 계수 (필요에 따라 인스펙터에서 조절)
    [SerializeField] private float windInfluenceMultiplier = 1.0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stabilizer = GetComponent<ArrowStabilizer>(); // ArrowStabilizer 컴포넌트를 찾아옴
    }

    // 추가: 물리 연산은 FixedUpdate에서 처리
    void FixedUpdate()
    {
        // 충돌했거나, 아직 게임 매니저가 없거나, 리지드바디가 멈췄다면 패스
        if (hasCollided || rb.isKinematic || GameManager.Instance == null) return;

        // 현재 설정된 바람 벡터 가져오기
        Vector3 wind = GameManager.Instance.windVelocity.Value;

        // 화살에 지속적인 힘을 가함 (ForceMode.Force는 질량의 영향을 받음)
        // 바람이 세면 화살이 휘어지는 효과가 납니다.
        rb.AddForce(wind * windInfluenceMultiplier, ForceMode.Force);
    }

    // 다른 오브젝트와 충돌하는 순간 한 번 호출됩니다.
    void OnCollisionEnter(Collision collision)
    {
        // 아직 충돌한 적이 없다면
        if (!hasCollided)
        {
            hasCollided = true;

            // 비행 안정화 기능을 즉시 꺼버립니다.
            if (stabilizer != null)
            {
                stabilizer.enabled = false;
            }
        }
    }

    void Update()
    {
        // 일단 충돌한 후에만, 멈추는 것을 감지합니다.
        if (!hasCollided || rb.isKinematic) return;

        if (rb.linearVelocity.magnitude < 0.1f && rb.angularVelocity.magnitude < 0.1f)
        {
            sleepTimer += Time.deltaTime;
        }
        else
        {
            sleepTimer = 0f;
        }

        if (sleepTimer >= timeUntilSleep)
        {
            rb.isKinematic = true;
        }
    }
}