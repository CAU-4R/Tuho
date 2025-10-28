using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요
using System.Collections; // 코루틴을 사용하기 위해 필요

public class UIManager : MonoBehaviour
{
    public TMP_Text scoreText;
    public GameObject successTextObject;

    // 점수 텍스트를 업데이트하는 함수
    public void UpdateScore(int score)
    {
        scoreText.text = "Score: " + score;
    }

    // 성공 메시지를 보여주는 함수
    public void ShowSuccessMessage()
    {
        StartCoroutine(ShowAndHideSuccessText());
    }

    // 메시지를 1.5초 동안 보여주고 자동으로 숨기는 코루틴
    private IEnumerator ShowAndHideSuccessText()
    {
        successTextObject.SetActive(true); // 메시지 켜기
        yield return new WaitForSeconds(1.5f); // 1.5초 기다리기
        successTextObject.SetActive(false); // 메시지 끄기
    }
}