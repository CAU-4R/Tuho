using UnityEngine;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject coverageCanvas;
    public GameObject startCanvas;
    public GameObject hostCanvas;
    public GameObject clientCanvas;
    public GameObject gameCanvas;
    public GameObject quitCanvas;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        StartGameAR.OnStartSharedSpaceHost += HandleHostSelected;
        StartGameAR.OnJoinSharedSpaceClient += HandleClientSelected;
        StartGameAR.OnEnterGameCanvas += HandleEnterGameCanvas;
        StartGameAR.OnStartGame += HandleGameStart;
    }

    private void OnDisable()
    {
        StartGameAR.OnStartSharedSpaceHost -= HandleHostSelected;
        StartGameAR.OnJoinSharedSpaceClient -= HandleClientSelected;
        StartGameAR.OnEnterGameCanvas -= HandleEnterGameCanvas;
        StartGameAR.OnStartGame -= HandleGameStart;
    }

    void Start()
    {
        StartCoroutine(CoverageSequence());
    }

    private IEnumerator CoverageSequence()
    {
        SetOnly(coverageCanvas);
        yield return new WaitForSeconds(5f);
        ShowStart();
    }

    public void ShowStart()
    {
        SetOnly(startCanvas);
    }

    private void HandleHostSelected()
    {
        ShowHost();
    }

    private void HandleClientSelected()
    {
        ShowClient();
    }

    private void HandleEnterGameCanvas()
    {
        SetOnly(gameCanvas);
    }

    private void HandleGameStart()
    {
        // 게임 시작 시 UI에서 할 작업만 넣기
        // 타이머 시작은 TimerStartButton → TimerManager 서버 RPC
    }

    public void ShowHost()
    {
        SetOnly(hostCanvas);
    }

    public void ShowClient()
    {
        SetOnly(clientCanvas);
    }

    public void ShowQuitCanvas()
    {
        SetOnly(quitCanvas);
    }

    private void SetOnly(GameObject target)
    {
        coverageCanvas.SetActive(false);
        startCanvas.SetActive(false);
        hostCanvas.SetActive(false);
        clientCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        quitCanvas.SetActive(false);

        target.SetActive(true);
    }
}
