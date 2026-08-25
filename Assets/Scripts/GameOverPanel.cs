using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI reasonText;
    [SerializeField] private Button restartButton;
    [SerializeField] private GameObject panelRoot;

    void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.StartNewGame();
            });
        }
    }

    void OnEnable()
    {
        GameEvents.OnGameOver += ShowGameOver;
    }

    void OnDisable()
    {
        GameEvents.OnGameOver -= ShowGameOver;
    }

    private void ShowGameOver(GameOverReason reason)
    {
        if (panelRoot != null) panelRoot.SetActive(true);

        if (reasonText != null)
        {
            switch (reason)
            {
                case GameOverReason.Arrest:
                    reasonText.text = "You were arrested for possessing a stolen item!";
                    break;
                case GameOverReason.Bankruptcy:
                    reasonText.text = "You went bankrupt! Your balance fell below $0.";
                    break;
                case GameOverReason.MissedQuota:
                    reasonText.text = "You didn't hit the target profit for the day!";
                    break;
            }
        }
    }
}
