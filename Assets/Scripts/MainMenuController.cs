using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [Tooltip("Drag the 3D Text objects (START, TUTORIAL, QUIT) or their parent here to hide them when the game starts.")]
    [SerializeField] private GameObject[] menuObjects;
    
    [Header("Camera Transition")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("The position the camera moves to when the game starts.")]
    [SerializeField] private Vector3 gameCameraPosition = new Vector3(-0.3f, 0f, -3.5f);
    
    [Tooltip("The rotation (in degrees) the camera moves to when the game starts.")]
    [SerializeField] private Vector3 gameCameraRotation = Vector3.zero;
    
    [SerializeField] private float transitionDuration = 2f;

    [Header("Dependencies")]
    [SerializeField] private DayManager dayManager;
    [SerializeField] private TutorialManager tutorialManager;

    private Vector3 menuCameraPosition;
    private Quaternion menuCameraRotation;

    private void Start()
    {
        // Capture the camera's current starting position to use as the Main Menu view
        if (mainCamera != null)
        {
            menuCameraPosition = mainCamera.transform.localPosition;
            menuCameraRotation = mainCamera.transform.localRotation;
        }

        // If we are on Day 1, show the menu. Otherwise, hide it because we are mid-game!
        if (GameManager.Instance != null && GameManager.Instance.currentDay > 1)
        {
            HideMenuInstantly();
        }
        else
        {
            ShowMenu();
        }
    }

    private void ShowMenu()
    {
        if (menuObjects != null)
        {
            foreach (var obj in menuObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }

        // Put camera at menu position
        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = menuCameraPosition;
            mainCamera.transform.localRotation = menuCameraRotation;
        }
    }

    private void HideMenuInstantly()
    {
        if (menuObjects != null)
        {
            foreach (var obj in menuObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        if (mainCamera != null)
        {
            mainCamera.transform.localPosition = gameCameraPosition;
            mainCamera.transform.localRotation = Quaternion.Euler(gameCameraRotation);
        }
    }

    // Called by the START button in the UI
    public void StartGame()
    {
        if (menuObjects != null)
        {
            foreach (var obj in menuObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
        StartCoroutine(TransitionToGame(false));
    }

    // Called by the TUTORIAL button in the UI
    public void StartTutorial()
    {
        if (menuObjects != null)
        {
            foreach (var obj in menuObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
        StartCoroutine(TransitionToGame(true));
    }

    // Called by the QUIT button in the UI
    public void QuitGame()
    {
        Debug.Log("[MainMenu] Quit Game");
        Application.Quit();
    }

    private IEnumerator TransitionToGame(bool isTutorial)
    {
        // Lerp camera from menu pos to game pos
        if (mainCamera != null)
        {
            float elapsed = 0f;
            Vector3 startPos = mainCamera.transform.localPosition;
            Quaternion startRot = mainCamera.transform.localRotation;
            Quaternion targetRot = Quaternion.Euler(gameCameraRotation);

            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, elapsed / transitionDuration);
                
                mainCamera.transform.localPosition = Vector3.Lerp(startPos, gameCameraPosition, t);
                mainCamera.transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
                
                yield return null;
            }

            mainCamera.transform.localPosition = gameCameraPosition;
            mainCamera.transform.localRotation = targetRot;
        }

        if (isTutorial)
        {
            if (tutorialManager != null) tutorialManager.StartTutorial();
        }
        else
        {
            // Now tell the DayManager to begin Day 1 WITHOUT the intro!
            if (dayManager != null) dayManager.StartDayCycle(true);
        }
    }
}
