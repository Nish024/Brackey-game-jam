using UnityEngine;

/// <summary>
/// Manages the OS navigation of the phone (Home screen, opening apps, and the back button).
/// Attach this to the main Phone Canvas or a dedicated PhoneUI Manager object.
/// </summary>
public class PhoneAppManager : MonoBehaviour
{
    [Header("Screens")]
    [Tooltip("The main screen containing the app icons.")]
    [SerializeField] private GameObject homeScreen;
    
    [Tooltip("Drag all your app panels (Phone, Rules, News, etc.) in here.")]
    [SerializeField] private GameObject[] appScreens;

    [Header("Global UI")]
    [Tooltip("The back button that appears when inside an app.")]
    [SerializeField] private GameObject backButton;

    private void Start()
    {
        // Always start on the home screen when the game begins
        GoToHome();
    }

    /// <summary>
    /// Call this from your App Icon buttons. Drag the specific App panel into the parameter.
    /// </summary>
    public void OpenApp(GameObject appToOpen)
    {
        if (homeScreen != null) homeScreen.SetActive(false);

        // Turn off all apps, then turn on only the one we want to open
        foreach (var app in appScreens)
        {
            if (app != null)
            {
                app.SetActive(app == appToOpen);
            }
        }

        if (backButton != null) backButton.SetActive(true);
    }

    /// <summary>
    /// Call this from your Back Button.
    /// </summary>
    public void GoToHome()
    {
        if (homeScreen != null) homeScreen.SetActive(true);

        // Turn off all apps
        foreach (var app in appScreens)
        {
            if (app != null) app.SetActive(false);
        }

        // Hide back button on the home screen
        if (backButton != null) backButton.SetActive(false);
    }
}
