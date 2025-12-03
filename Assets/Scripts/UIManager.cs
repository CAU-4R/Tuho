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

    public Timer gameTimer;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        // StartGameAR 이벤트 구독
        StartGameAR.OnStartSharedSpaceHost += HandleHostSelected;
        StartGameAR.OnJoinSharedSpaceClient += HandleClientSelected;
        StartGameAR.OnEnterGameCanvas += HandleEnterGameCanvas;
        StartGameAR.OnStartGame += HandleGameStart;
    }

    private void OnDisable()
    {
        // 이벤트 해제
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
        SetOnly(gameCanvas);  // UI 이동만
    }


    private void HandleGameStart()
    {
        if (gameTimer != null)
            gameTimer.StartTimer();
    }

    public void ShowHost()
    {
        SetOnly(hostCanvas);
    }

    public void ShowClient()
    {
        SetOnly(clientCanvas);
    }

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