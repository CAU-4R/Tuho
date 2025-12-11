using UnityEngine;
using TMPro;
using System.Collections;

public class Timer : MonoBehaviour
{
    public System.Action OnTimerEnd;

    [Header("Main Timer")]
    public float timeValue = 90f;
    public TMP_Text timerText;

    [Header("Countdown Before Start")]
    public TMP_Text countdownText;
    public float preStartCountdown = 5f;

    private bool isRunning = false;
    private bool endTriggered = false;

    private Color defaultColor;   // 원래 색 저장용


    void Start()
    {
        if (timerText != null)
            defaultColor = timerText.color;

        if (countdownText != null)
            countdownText.gameObject.SetActive(false);

        if (timerText != null)
            timerText.gameObject.SetActive(true);

        DisplayTime(timeValue);
    }


    void Update()
    {
        if (!isRunning) return;

        if (timeValue > 0)
        {
            timeValue -= Time.deltaTime;

            // 시간 음수 방지
            timeValue = Mathf.Max(timeValue, 0f);
        }
        else
        {
            timeValue = 0f;

            if (!endTriggered)
            {
                endTriggered = true;
                isRunning = false;
                OnTimerEnd?.Invoke();
            }
        }

        DisplayTime(timeValue);
    }

    public void StartTimer()
    {
        StopAllCoroutines();
        StartCoroutine(StartCountdownThenTimer());
    }

    private IEnumerator StartCountdownThenTimer()
    {
        float count = preStartCountdown;

        countdownText.gameObject.SetActive(true);

        while (count > 0)
        {
            countdownText.text = Mathf.CeilToInt(count).ToString();
            yield return new WaitForSeconds(1f);
            count -= 1f;
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        isRunning = true;
        endTriggered = false;
    }

    public void ResetTimer(float newTime = 90f)
    {
        StopAllCoroutines();
        timeValue = newTime;
        isRunning = false;
        endTriggered = false;

        DisplayTime(timeValue);

        countdownText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(true);

        // 색 초기화
        if (timerText != null)
            timerText.color = defaultColor;
    }

private void DisplayTime(float timeToDisplay)
{
    // 무조건 먼저 0 이하 방지
    if (timeToDisplay <= 0f)
    {
        timerText.text = "00:00";
        timerText.color = Color.red;
        return;
    }

    int totalSeconds = Mathf.FloorToInt(timeToDisplay);
    int minutes = totalSeconds / 60;
    int seconds = totalSeconds % 60;

    timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

    // 7초 이하 빨간색
    if (timeToDisplay <= 7f)
        timerText.color = Color.red;
    else
        timerText.color = defaultColor;
}


}
