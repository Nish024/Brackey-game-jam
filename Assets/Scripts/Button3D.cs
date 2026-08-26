using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A simple 3D physical button script.
/// Attach this to a 3D object (like a Cube) with a Collider.
/// Use the OnClick UnityEvent in the Inspector to hook up actions (like TransactionController.Buy).
/// </summary>
[RequireComponent(typeof(Collider))]
public class Button3D : MonoBehaviour
{
    [Header("Events")]
    [Tooltip("What happens when this 3D button is clicked.")]
    public UnityEvent onClick;

    [Tooltip("If false, the button will not respond to clicks or animate.")]
    public bool interactable = true;

    [Header("Visual Feedback")]
    [Tooltip("How far the button moves down when pressed. (Local space)")]
    [SerializeField] private float pressDepth = 0.05f;
    
    [Tooltip("How fast the button animates.")]
    [SerializeField] private float animationSpeed = 15f;

    [Tooltip("Which local axis does the button press down on? (Usually Y for flat buttons, or Z for wall buttons)")]
    [SerializeField] private Vector3 pressAxis = Vector3.down;

    private Vector3 originalLocalPos;
    private Vector3 pressedLocalPos;
    private bool isPressed = false;

    void Start()
    {
        originalLocalPos = transform.localPosition;
        pressedLocalPos = originalLocalPos + (pressAxis.normalized * pressDepth);
    }

    void Update()
    {
        // Smoothly animate the button's position
        Vector3 targetPos = isPressed ? pressedLocalPos : originalLocalPos;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * animationSpeed);
    }

    // Called when the mouse clicks down on the collider
    void OnMouseDown()
    {
        isPressed = true;
    }

    // Called when the mouse is released over the SAME collider it clicked on
    void OnMouseUpAsButton()
    {
        onClick?.Invoke();
    }

    // Called when the mouse is released anywhere (to reset the visual state)
    void OnMouseUp()
    {
        isPressed = false;
    }
}
