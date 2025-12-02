using UnityEngine;
using TMPro;
using Febucci.TextAnimatorForUnity.TextMeshPro;

public class Timer : MonoBehaviour
{
    public System.Action OnTimerEnd;
    public float timeValue = 90;

    public TMP_Text timerText;
    public TextAnimator_TMP textAnimator;

    private bool isRunning = false;
    private bool endTriggered = false;

    // 🔥 흔들림 효과가 여러 번 적용되지 않도록 체크하는 변수
    private bool shakeApplied = false;

    void Update()
    {
        if (!isRunning) return;

        if (timeValue > 0)
            timeValue -= Time.deltaTime;
        else
        {
            timeValue = 0;

            if (!endTriggered)
            {
                endTriggered = true;
                isRunning = false;

                OnTimerEnd?.Invoke();
            }
        }

        DisplayTime(timeValue);
    }

    void DisplayTime(float timeToDisplay)
    {
        if (timeToDisplay < 0) timeToDisplay = 0;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        string text = $"{minutes:00}:{seconds:00}";

        // 🔥 Text Animator로 출력
        textAnimator.SetText(text);

        // 🔥 7초 이하일 때 shake 효과 적용 (딱 한 번만)
        if (seconds <= 7 && !shakeApplied)
        {
            shakeApplied = true;
            textAnimator.SetText($"<shake>{text}</shake>");
        }
    }

    public void StartTimer()
    {
        isRunning = true;
        endTriggered = false;
        shakeApplied = false;
    }

    public void ResetTimer(float newTime = 90)
    {
        timeValue = newTime;
        isRunning = false;
        endTriggered = false;
        shakeApplied = false;

        timerText.text = $"{Mathf.FloorToInt(newTime / 60):00}:{Mathf.FloorToInt(newTime % 60):00}";
    }
}
