using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Controls the physical 3D Loan / Billing Machine on the desk.
/// Moves between idlePos and viewPos when clicked.
/// Listens for 3D collider clicks on +, -, and confirm buttons.
/// Updates the machine's 3D screen display.
/// </summary>
[RequireComponent(typeof(Collider))]
public class LoanMachineController : MonoBehaviour
{
    [Header("Positions")]
    [Tooltip("Rest position on the desk.")]
    [SerializeField] private Transform idlePos;
    [Tooltip("View position in front of the camera (can reuse the Phone view position).")]
    [SerializeField] private Transform viewPos;
    [Tooltip("Added to the viewPos rotation. E.g. (0, 180, 0) to flip it around!")]
    [SerializeField] private Vector3 viewRotationOffset = new Vector3(0, 180f, 0);

    [Header("3D Button Colliders")]
    [Tooltip("Collider for the '+' button on the 3D model.")]
    [SerializeField] private Collider plusCollider;
    [Tooltip("Collider for the '-' button on the 3D model.")]
    [SerializeField] private Collider minusCollider;
    [Tooltip("Collider for the 'confirm' button on the 3D model.")]
    [SerializeField] private Collider confirmCollider;
    [Tooltip("Collider for the 'back' button on the 3D model.")]
    [SerializeField] private Collider backCollider;

    [Tooltip("Collider for the main machine body to pick it up.")]
    [SerializeField] private Collider mainBodyCollider;

    [Header("Screen Display")]
    [Tooltip("TextMeshPro object for the loan amount.")]
    [SerializeField] private TextMeshPro screenText;
    [SerializeField] private TextMeshProUGUI screenTextUGUI;
    
    [Tooltip("TextMeshPro object for the interest rate and repayment amount.")]
    [SerializeField] private TextMeshPro interestRateText;
    [SerializeField] private TextMeshProUGUI interestRateTextUGUI;

    [Header("References")]
    [SerializeField] private LoanManager loanManager;
    [SerializeField] private float moveSpeed = 8f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSfx;

    private bool isAtView = false;
    private Coroutine currentMove;

    public bool IsAtView => isAtView;

    void Start()
    {
        if (loanManager == null) loanManager = FindObjectOfType<LoanManager>();

        if (idlePos != null)
        {
            transform.position = idlePos.position;
            transform.rotation = idlePos.rotation;
        }

        UpdateScreenDisplay();
    }

    public void MoveToView()
    {
        if (viewPos == null || isAtView) return;

        // Verify with LoanManager that we are ACTUALLY allowed to take a loan right now
        // (Must have a customer, can't afford item, haven't used loan today)
        if (loanManager != null && !loanManager.TryOpenLoanPanel())
        {
            // The loan manager blocked it, so don't pick it up!
            return;
        }

        // Drop gun, phone, or clipboard if currently being held
        var pickup = FindObjectOfType<Pickup>();
        if (pickup != null) pickup.ForceReturnItem();

        var phone = FindObjectOfType<PhoneController>();
        if (phone != null && phone.IsAtView) phone.MoveToIdle();

        var clipboard = FindObjectOfType<ClipboardController>();
        if (clipboard != null && clipboard.IsAtView) clipboard.MoveToIdle();

        isAtView = true;

        UpdateScreenDisplay();
        
        // Apply the 180 degree rotation offset!
        Quaternion targetRotation = viewPos.rotation * Quaternion.Euler(viewRotationOffset);
        BeginMove(viewPos.position, targetRotation);
    }

    public void MoveToIdle()
    {
        if (idlePos == null || !isAtView) return;

        isAtView = false;
        BeginMove(idlePos.position, idlePos.rotation);
    }

    public void UpdateScreenDisplay()
    {
        if (loanManager == null) return;

        int amount = loanManager.CurrentLoanAmount;
        float rate = loanManager.CurrentInterestRate();
        float repayment = Mathf.Round(amount * (1f + rate));

        // Format Loan Amount
        string amountString = $"${amount}";
        if (screenText != null) screenText.text = amountString;
        if (screenTextUGUI != null) screenTextUGUI.text = amountString;

        // Format Interest & Repayment
        string rateString = amount == 0 
            ? $"Rate: {rate * 100f:F0}%" 
            : $"Repay: ${repayment:F0}\n({rate * 100f:F0}%)";
            
        if (interestRateText != null) interestRateText.text = rateString;
        if (interestRateTextUGUI != null) interestRateTextUGUI.text = rateString;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    private void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Length == 0) return;

        bool hitMainBody = false;

        // First, check if we hit any of the specific buttons (RaycastAll ignores order, so we check everything hit)
        foreach (var hit in hits)
        {
            if (!isAtView)
            {
                // If we're not at view, ANY click on the machine or buttons should pick it up
                if (hit.collider == mainBodyCollider || hit.collider == plusCollider || 
                    hit.collider == minusCollider || hit.collider == confirmCollider || hit.collider == backCollider)
                {
                    MoveToView();
                    return; // Picked up, stop processing
                }
            }
            else
            {
                // We are at view position, check if a specific button was clicked
                if (plusCollider != null && hit.collider == plusCollider)
                {
                    PlayButtonSound();
                    loanManager?.OnPlus();
                    UpdateScreenDisplay();
                    return;
                }
                else if (minusCollider != null && hit.collider == minusCollider)
                {
                    PlayButtonSound();
                    loanManager?.OnMinus();
                    UpdateScreenDisplay();
                    return;
                }
                else if (confirmCollider != null && hit.collider == confirmCollider)
                {
                    PlayButtonSound();
                    loanManager?.OnConfirmLoan();
                    UpdateScreenDisplay();
                    MoveToIdle();
                    return;
                }
                else if (backCollider != null && hit.collider == backCollider)
                {
                    PlayButtonSound();
                    MoveToIdle();
                    return;
                }
            }

            if (hit.collider == mainBodyCollider) hitMainBody = true;
        }

        // If we get here, no buttons were clicked. 
        // If we clicked the main body while at view, we can just ignore it (or you could put it down).
    }

    private void BeginMove(Vector3 targetPos, Quaternion targetRot)
    {
        if (currentMove != null) StopCoroutine(currentMove);
        currentMove = StartCoroutine(LerpToPoint(targetPos, targetRot));
    }

    private IEnumerator LerpToPoint(Vector3 targetPos, Quaternion targetRot)
    {
        Vector3 startP = transform.position;
        Quaternion startR = transform.rotation;
        float dist = Vector3.Distance(startP, targetPos);

        if (dist < 0.01f)
        {
            transform.SetPositionAndRotation(targetPos, targetRot);
            yield break;
        }

        float duration = dist / moveSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            
            transform.position = Vector3.Lerp(startP, targetPos, t);
            transform.rotation = Quaternion.Slerp(startR, targetRot, t);
            
            yield return null;
        }

        transform.SetPositionAndRotation(targetPos, targetRot);
    }

    private void PlayButtonSound()
    {
        if (audioSource != null && buttonSfx != null)
        {
            audioSource.PlayOneShot(buttonSfx);
        }
    }
}
