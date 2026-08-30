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
            restartButton.onClick.AddListener(OnRestartClicked);
        }
    }

    public void OnRestartClicked()
    {
        Debug.Log("[GameOverPanel] Restart button clicked.");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }

    public void ShowGameOver(GameOverReason reason)
    {
        // Ensure the root object is active if it was disabled in the inspector
        gameObject.SetActive(true);

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling(); // Force render on top of BlackScreen
        }

        if (reasonText != null)
        {
            reasonText.gameObject.SetActive(true); // Ensure it's active in case it was off in Inspector
            reasonText.enabled = true; // Ensure the component is ticked on
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
                case GameOverReason.LoanNotRepaid:
                    reasonText.text = "You couldn't repay your loan! The bank has seized your shop.";
                    break;
                case GameOverReason.TooManyFakes:
                    reasonText.text = "FIRED! You bought too many fake guns and ruined the shop's reputation.";
                    break;
            }
        }
    }

    public void ShowVictory()
    {
        gameObject.SetActive(true);

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
            panelRoot.transform.SetAsLastSibling();
        }

        if (reasonText != null)
        {
            reasonText.gameObject.SetActive(true);
            reasonText.enabled = true;
            reasonText.text = "VICTORY! You survived all 3 days and successfully managed the shop!";
        }
    }
}
