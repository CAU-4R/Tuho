using UnityEngine;
using System.Collections;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject coverageCanvas;
    public GameObject startCanvas;
    public GameObject hostCanvas;
    public GameObject clientCanvas;
    public GameObject gameCanvas;

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

void Start()
{
    StartCoroutine(CoverageSequence());
}

private IEnumerator CoverageSequence()
{
    // 1) Coverage 켜기
    SetOnly(coverageCanvas);

    // 2) 5초 기다리기
    yield return new WaitForSeconds(5f);

    // 3) Start Canvas로 이동
    ShowStart();
}


    public void ShowStart()
    {
        SetOnly(startCanvas);
    }

    public void ShowHost()
    {
        SetOnly(hostCanvas);
    }

    public void ShowClient()
    {
        SetOnly(clientCanvas);
    }

    public void RegisterAsHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("HOST 시작됨");
        
        // 게임 UI로 전환하고 싶다면 여기에 추가
        SetOnly(gameCanvas);
    }

    public void RegisterAsClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("CLIENT 접속 시도");
        
        // 게임 UI로 전환하고 싶다면 여기에 추가
        SetOnly(gameCanvas);
    }

    // 하나만 켜고 나머지는 모두 끄기
    private void SetOnly(GameObject target)
    {
        coverageCanvas.SetActive(false);
        startCanvas.SetActive(false);
        hostCanvas.SetActive(false);
        clientCanvas.SetActive(false);
        gameCanvas.SetActive(false);

        target.SetActive(true);
    }
}
