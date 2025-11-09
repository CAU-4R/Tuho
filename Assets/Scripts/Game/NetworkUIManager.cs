using System.Collections;
using System.Linq;
using Unity.Netcode;
using TMPro;
using UnityEngine;

public class NetworkUIManager : NetworkBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject successTextObject;

    [Header("Game Control UI")]
    [SerializeField] private Canvas createGameCanvas;
    [SerializeField] private Canvas controllerCanvas;
    [SerializeField] private Canvas restartQuitCanvas;

    private void Start()
    {
        // 초기 점수/성공 메시지 UI
        if (successTextObject != null)
            successTextObject.SetActive(false);

        // CreateGameCanvas 초기 상태
        ShowCreateGameCanvas();

        // AllPlayerDataManager 이벤트 구독
        if (AllPlayerDataManager.Instance != null)
            AllPlayerDataManager.Instance.OnPlayerScoreChanged += HandlePlayerScoreChanged;

        if (AllPlayerDataManager.Instance != null)
            AllPlayerDataManager.Instance.OnPlayerDead += HandlePlayerDead;
    }

    private void OnDestroy()
    {
        if (AllPlayerDataManager.Instance != null)
        {
            AllPlayerDataManager.Instance.OnPlayerScoreChanged -= HandlePlayerScoreChanged;
            AllPlayerDataManager.Instance.OnPlayerDead -= HandlePlayerDead;
        }
    }

    #region Game Start / UI

    public void OnCreateGameClicked()
    {
        if (IsServer)
            StartGameServerRpc();
        else
            RequestStartGameServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestStartGameServerRpc(ServerRpcParams rpcParams = default) => StartGameServerRpc();

    [ServerRpc(RequireOwnership = false)]
    private void StartGameServerRpc() => ShowPlayerControlsClientRpc();

    [ClientRpc]
    private void ShowPlayerControlsClientRpc()
    {
        createGameCanvas.gameObject.SetActive(false);
        controllerCanvas.gameObject.SetActive(true);
        restartQuitCanvas.gameObject.SetActive(false);
    }

    #endregion

    #region Score & Success Message

    private void HandlePlayerScoreChanged(ulong clientId)
    {
        var allScores = AllPlayerDataManager.Instance.GetAllScores();
        if (scoreText != null)
        {
            scoreText.text = string.Join("\n", allScores.Select(kvp => $"Player {kvp.Key}: {kvp.Value}"));
        }
        ShowSuccessMessage();
    }

    public void ShowSuccessMessage()
    {
        if (IsServer)
            ShowSuccessMessageClientRpc();
        else
            ShowSuccessMessageServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShowSuccessMessageServerRpc() => ShowSuccessMessageClientRpc();

    [ClientRpc]
    private void ShowSuccessMessageClientRpc()
    {
        if (successTextObject != null)
            StartCoroutine(ShowAndHideSuccessText());
    }

    private IEnumerator ShowAndHideSuccessText()
    {
        successTextObject.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        successTextObject.SetActive(false);
    }

    #endregion

    #region Player Dead / Restart

    private void HandlePlayerDead(ulong clientId)
    {
        if (IsServer)
            PlayerIsDeadClientRpc();
    }

    [ClientRpc]
    private void PlayerIsDeadClientRpc()
    {
        createGameCanvas.gameObject.SetActive(false);
        controllerCanvas.gameObject.SetActive(false);
        restartQuitCanvas.gameObject.SetActive(true);
    }

    public void OnRestartGame()
    {
        if (IsServer)
            ShowPlayerControlsClientRpc();
        else
            RequestStartGameServerRpc();
    }

    #endregion

    #region Initial UI

    private void ShowCreateGameCanvas()
    {
        createGameCanvas.gameObject.SetActive(true);
        controllerCanvas.gameObject.SetActive(false);
        restartQuitCanvas.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // 네트워크 스폰 시 UI: ControllerCanvas 활성
        createGameCanvas.gameObject.SetActive(false);
        controllerCanvas.gameObject.SetActive(true);
        restartQuitCanvas.gameObject.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        StopAllCoroutines();
        if (AllPlayerDataManager.Instance != null)
        {
            AllPlayerDataManager.Instance.OnPlayerScoreChanged -= HandlePlayerScoreChanged;
            AllPlayerDataManager.Instance.OnPlayerDead -= HandlePlayerDead;
        }
    }

    #endregion
}
