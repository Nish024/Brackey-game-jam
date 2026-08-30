using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The parent object for the Tutorial UI (Text and Return button)")]
    public GameObject tutorialCanvas;
    public TextMeshProUGUI explanationText;
    public Button menuButton;
    public string defaultText = "If done, click on MAIN MENU";

    [Header("Customer & Item")]
    [Tooltip("A customer prefab to stand in front of the counter for the tutorial")]
    public GameObject singleCustomerPrefab;
    [Tooltip("Where the customer will stand")]
    public Transform counterSpawnPoint; 
    
    [Space]
    [Tooltip("A sample gun prefab to spawn on the counter for the tutorial")]
    public GameObject sampleItemPrefab;
    [Tooltip("Where the item rests on the counter")]
    public Transform itemCounterPos;
    [Tooltip("Where the item floats when inspected (in front of camera)")]
    public Transform itemViewPos;

    [Space]
    [Tooltip("The clipboard prefab to spawn on the counter")]
    public GameObject sampleClipboardPrefab;
    [Tooltip("Where the clipboard rests on the counter")]
    public Transform clipboardCounterPos;
    [Tooltip("Where the clipboard floats when inspected")]
    public Transform clipboardViewPos;

    private void Awake()
    {
        if (tutorialCanvas != null) tutorialCanvas.SetActive(false);
        if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);
    }

    public void StartTutorial()
    {
        Debug.Log("[Tutorial] Starting tutorial mode.");
        
        // Show tutorial UI
        if (tutorialCanvas != null) tutorialCanvas.SetActive(true);
        if (explanationText != null) explanationText.text = defaultText;

        // Spawn exactly one customer at the counter (they will just stand there)
        if (singleCustomerPrefab != null && counterSpawnPoint != null)
        {
            Instantiate(singleCustomerPrefab, counterSpawnPoint.position, counterSpawnPoint.rotation);
        }

        // Spawn a sample item so the player can interact with it
        if (sampleItemPrefab != null && itemCounterPos != null)
        {
            GameObject itemObj = Instantiate(sampleItemPrefab, itemCounterPos.position, itemCounterPos.rotation);
            ItemController ic = itemObj.GetComponent<ItemController>();
            
            if (ic != null && itemViewPos != null)
            {
                // Init uses: Counter, View, Bought, Speed
                ic.Init(itemCounterPos, itemViewPos, itemCounterPos, 5f);
                ic.MoveToCounter(); // Sets state to AtCounter since it's already there
            }
        }

        // Spawn the clipboard so the player can interact with it
        if (sampleClipboardPrefab != null && clipboardCounterPos != null && clipboardViewPos != null)
        {
            GameObject clipboardObj = Instantiate(sampleClipboardPrefab, clipboardCounterPos.position, clipboardCounterPos.rotation);
            ClipboardController cc = clipboardObj.AddComponent<ClipboardController>();
            
            if (cc != null)
            {
                // Init uses: Start, Idle, View, Speed
                cc.Init(clipboardCounterPos, clipboardCounterPos, clipboardViewPos, 5f);
                cc.MoveToIdle();
            }
        }
    }

    private void ReturnToMenu()
    {
        // Go back to the main menu by simply reloading the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        // Only run hover logic if tutorial is active
        if (tutorialCanvas == null || !tutorialCanvas.activeSelf) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // Raycast against everything to find TutorialHoverItems
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            TutorialHoverItem hover = hit.collider.GetComponentInParent<TutorialHoverItem>();
            if (hover != null)
            {
                if (explanationText != null) explanationText.text = hover.descriptionText;
                return;
            }
        }
        
        // If we hit nothing (or hit something without the script), revert to default
        if (explanationText != null) explanationText.text = defaultText;
    }
}
