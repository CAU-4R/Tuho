using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public float timeValue = 90;
    public TMP_Text timerText;
    private bool isRunning = false;

    void Update()
    {
        if (!isRunning) return;

        if (timeValue > 0)
            timeValue -= Time.deltaTime;
        else
            timeValue = 0;

        DisplayTime(timeValue);
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void ResetTimer(float newTime = 90)
    {
        timeValue = newTime;
        isRunning = false;
        DisplayTime(timeValue);
    }

    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0)
            timeToDisplay = 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}