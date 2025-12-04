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


    void Start()
    {
        if(countdownText != null)
            countdownText.gameObject.SetActive(false);

        if(timerText != null)
            timerText.gameObject.SetActive(true);

        DisplayTime(timeValue);
    }


    void Update()
    {
        if (!isRunning) return;

        if (timeValue > 0)
        {
            timeValue -= Time.deltaTime;
        }
        else
        {
            timeValue = 0;

            if (!endTriggered)
            {
                endTriggered = true;
                isRunning = false;

                OnTimerEnd?.Invoke();  // UIManager로 알려줌
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

        // 5 → 4 → 3 → 2 → 1 표시
        while (count > 0)
        {
            countdownText.text = Mathf.CeilToInt(count).ToString();
            yield return new WaitForSeconds(1f);
            count -= 1f;
        }

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
    }


    private void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0)
            timeToDisplay = 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
