using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple component you can attach to any UI Button in the Inspector.
/// It will automatically trigger the correct sound from the AudioManager when clicked.
/// </summary>
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    public enum SoundType 
    { 
        PhoneButton, 
        NextModel, 
        MainMenu 
    }
    
    [Tooltip("Which sound should play when this button is clicked?")]
    public SoundType soundType = SoundType.PhoneButton;

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(PlaySound);
        }
    }

    private void PlaySound()
    {
        if (AudioManager.Instance == null) return;
        
        switch (soundType)
        {
            case SoundType.PhoneButton: 
                AudioManager.Instance.PlayPhoneButtonSound(); 
                break;
            case SoundType.NextModel: 
                AudioManager.Instance.PlayNextModelSound(); 
                break;
            case SoundType.MainMenu: 
                AudioManager.Instance.PlayMainMenuSound(); 
                break;
        }
    }
}
