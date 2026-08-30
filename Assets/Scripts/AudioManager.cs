using UnityEngine;

/// <summary>
/// A global singleton to handle all audio clips and sources.
/// Attach this to a persistent GameObject in your scene (e.g. "GameLogic").
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Audio source for background music")]
    public AudioSource bgmSource;
    [Tooltip("Audio source for sound effects (clicks, etc.)")]
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    [Tooltip("Music or ambient sound for the main menu")]
    public AudioClip mainMenuSound;
    [Tooltip("Sound played when the next model/customer is summoned")]
    public AudioClip nextModelSound;
    [Tooltip("Sound played when pressing +, -, or confirm on the 3D Loan Machine")]
    public AudioClip machineButtonSound;
    [Tooltip("Sound played when tapping the Phone UI apps or buttons")]
    public AudioClip phoneButtonSound;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null); // Detach from parent to allow DontDestroyOnLoad without warnings
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Play main menu music/sound if it exists on start
        if (mainMenuSound != null && bgmSource != null)
        {
            bgmSource.clip = mainMenuSound;
            bgmSource.loop = true;
            bgmSource.Play();
        }
    }

    public void PlayMainMenuSound()
    {
        if (sfxSource != null && mainMenuSound != null)
            sfxSource.PlayOneShot(mainMenuSound);
    }

    public void PlayNextModelSound()
    {
        if (sfxSource != null && nextModelSound != null)
            sfxSource.PlayOneShot(nextModelSound);
    }

    public void PlayMachineButtonSound()
    {
        if (sfxSource != null && machineButtonSound != null)
            sfxSource.PlayOneShot(machineButtonSound);
    }

    public void PlayPhoneButtonSound()
    {
        if (sfxSource != null && phoneButtonSound != null)
            sfxSource.PlayOneShot(phoneButtonSound);
    }
}
