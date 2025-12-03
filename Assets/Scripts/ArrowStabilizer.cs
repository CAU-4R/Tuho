using UnityEngine;

// 이 스크립트는 Rigidbody가 있는 오브젝트에만 붙일 수 있도록 강제합니다.
[RequireComponent(typeof(Rigidbody))]
public class ArrowStabilizer : MonoBehaviour
{

    private Rigidbody rb;

    void Awake()
    {
        // 이 오브젝트의 Rigidbody 컴포넌트를 가져옵니다.
        rb = GetComponent<Rigidbody>();
    }

    // 물리 프레임마다 호출됩니다.
    void FixedUpdate()
    {
        // 화살의 속도가 0이 아닐 때만 (즉, 날아가는 중일 때만)
        if (rb.linearVelocity != Vector3.zero)
        {
            // 화살의 머리(transform.forward) 방향을
            // 현재 날아가는 방향(rb.linearVelocity)과 즉시 일치시킵니다.
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }
}