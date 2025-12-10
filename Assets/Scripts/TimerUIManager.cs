using UnityEngine;
using TMPro;
using System.Collections;

public class TimerUIManager : MonoBehaviour
{
    public TMP_Text timerText;
    public TMP_Text countdownText;
    public TimerManager timerManager;
    
    private IEnumerator Start()
    {
        while (TimerManager.Instance == null) 
            yield return null;

        timerManager = TimerManager.Instance;
    }

    void Update()
    {
        if (timerManager == null) return;

        // countdown
        if (timerManager.countdown.Value > 0)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = timerManager.countdown.Value.ToString();
        }
        else if (timerManager.countdown.Value == -1)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "GO!";
        }
        else
        {
            countdownText.gameObject.SetActive(false);
        }

        // main timer
        float t = timerManager.timeValue.Value;
        timerText.text = $"{Mathf.Floor(t/60):00}:{Mathf.Floor(t%60):00}";

        if (t <= 7f)
            timerText.color = Color.red;
    }
}
