using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ArrowPhysicsController : MonoBehaviour
{
    private Rigidbody rb;
    private ArrowStabilizer stabilizer; // ArrowStabilizer를 제어하기 위한 변수

    private float sleepTimer = 0f;
    private const float timeUntilSleep = 2f;
    private bool hasCollided = false; // 충돌 여부를 확인하는 깃발

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        stabilizer = GetComponent<ArrowStabilizer>(); // ArrowStabilizer 컴포넌트를 찾아옴
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