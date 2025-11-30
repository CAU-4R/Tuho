using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

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

    public void ShowGame()
    {
        SetOnly(gameCanvas);
    }

    // 하나만 켜고 나머지는 모두 끄기
    private void SetOnly(GameObject target)
    {
        startCanvas.SetActive(false);
        hostCanvas.SetActive(false);
        clientCanvas.SetActive(false);
        gameCanvas.SetActive(false);

        target.SetActive(true);
    }
}
