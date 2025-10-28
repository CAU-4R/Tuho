using UnityEngine;

public class ScoreDetector : MonoBehaviour
{
    private UIManager uiManager;
    private int score = 0;

    // 게임이 시작될 때 UIManager를 한 번만 찾아둡니다.
    void Start()
    {
        uiManager = FindAnyObjectByType<UIManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Arrow"))
        {
            ArrowState arrow = other.GetComponent<ArrowState>();
            if (arrow != null && !arrow.hasScored)
            {
                arrow.hasScored = true;

                // 점수를 올리고 UI 업데이트를 요청합니다.
                score++;
                if (uiManager != null)
                {
                    uiManager.UpdateScore(score);
                    uiManager.ShowSuccessMessage();
                }
            }
        }
    }
}