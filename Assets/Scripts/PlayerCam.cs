using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [Header("Sway Settings")]
    [Tooltip("How much the camera rotates on the X axis (up/down).")]
    public float swayAmountX = 10f;
    [Tooltip("How much the camera rotates on the Y axis (left/right).")]
    public float swayAmountY = 15f;
    [Tooltip("How smoothly the camera follows the cursor.")]
    public float smoothSpeed = 5f;

    private Quaternion baseRotation;

    private void Start()
    {
        // Store the initial rotation so we sway relative to it
        baseRotation = transform.localRotation;
    }

    private void Update()
    {
        // Get mouse position relative to the center of the screen
        // Range will be from -0.5 to 0.5
        float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
        float mouseY = (Input.mousePosition.y / Screen.height) - 0.5f;

        // Calculate target rotation based on mouse position
        // We invert mouseY because moving the mouse UP (positive Y) should rotate the camera UP (negative X rotation)
        Quaternion xSway = Quaternion.AngleAxis(-mouseY * swayAmountX, Vector3.right);
        Quaternion ySway = Quaternion.AngleAxis(mouseX * swayAmountY, Vector3.up);

        Quaternion targetRotation = baseRotation * xSway * ySway;

        // Smoothly rotate towards the target
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }
}
