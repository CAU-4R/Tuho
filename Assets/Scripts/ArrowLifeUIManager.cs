using UnityEngine;
using UnityEngine.UI;

public class ArrowLifeUI : MonoBehaviour
{
    [Header("UI Icons")]
    public Image[] arrowIcons;          // UI에 배치된 화살 이미지들

    [Header("Sprites")]
    public Sprite activeArrowSprite;    // 남아있는 화살(활성) 이미지
    public Sprite inactiveArrowSprite;  // 소모된 화살(비활성) 이미지

    [Header("Life Settings")]
    public int maxArrows = 5;
    public int currentArrows = 5;

    void Start()
    {
        UpdateArrows();
    }

    public void UpdateArrows()
    {
        for (int i = 0; i < arrowIcons.Length; i++)
        {
            if (i < currentArrows)
                arrowIcons[i].sprite = activeArrowSprite;     // 활성
            else
                arrowIcons[i].sprite = inactiveArrowSprite;   // 비활성
        }
    }

    public void ConsumeArrow()
    {
        if (currentArrows <= 0) return;
        currentArrows--;
        UpdateArrows();
    }

    public void AddArrow()
    {
        if (currentArrows >= maxArrows) return;
        currentArrows++;
        UpdateArrows();
    }
}
