using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages turning pages on and off inside the Rule App.
/// Only one page will be active at a time.
/// </summary>
public class RuleBookController : MonoBehaviour
{
    [Header("Pages")]
    [Tooltip("Drag all your page GameObjects in here in order!")]
    [SerializeField] private GameObject[] pages;

    [Header("Navigation Buttons")]
    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;

    private int currentPageIndex = 0;

    void Start()
    {
        if (nextPageButton != null) nextPageButton.onClick.AddListener(NextPage);
        if (prevPageButton != null) prevPageButton.onClick.AddListener(PrevPage);
        
        GoToPage(0);
    }

    void OnEnable()
    {
        // Always reset to the index page when opening the Rule App
        GoToPage(0);
    }

    public void NextPage()
    {
        GoToPage(currentPageIndex + 1);
    }

    public void PrevPage()
    {
        GoToPage(currentPageIndex - 1);
    }

    public void GoToPage(int index)
    {
        if (pages == null || pages.Length == 0) return;

        currentPageIndex = Mathf.Clamp(index, 0, pages.Length - 1);

        // Turn all pages off, then turn the target one on
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPageIndex);
            }
        }

        // Hide/Show Next and Prev buttons
        if (prevPageButton != null) prevPageButton.gameObject.SetActive(currentPageIndex > 0);
        if (nextPageButton != null) nextPageButton.gameObject.SetActive(currentPageIndex < pages.Length - 1);
    }

    /// <summary>
    /// Call this from a UI Button OnClick event. 
    /// Drag the GameObject of the page you want to open into the slot!
    /// </summary>
    public void GoToPageByGameObject(GameObject targetPage)
    {
        if (targetPage == null) return;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] == targetPage)
            {
                GoToPage(i);
                return;
            }
        }
    }
}
