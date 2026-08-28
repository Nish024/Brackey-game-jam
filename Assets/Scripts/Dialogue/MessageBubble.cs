using UnityEngine;
using TMPro;

/// <summary>
/// Put this on both the "Player" bubble prefab and the "RingExpert" (specialist) bubble prefab.
/// It just needs a name label and a message label - assign them in the inspector per prefab.
/// </summary>
public class MessageBubble : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text messageText;

    public void SetMessage(string speakerName, string message)
    {
        if (nameText != null) nameText.text = speakerName;
        if (messageText != null) messageText.text = message;
    }
}
