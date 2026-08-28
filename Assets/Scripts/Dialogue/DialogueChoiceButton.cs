using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Put this on a Button prefab that has a TMP_Text child for the choice label.
/// Instantiated at runtime, one per available player choice.
/// </summary>
[RequireComponent(typeof(Button))]
public class DialogueChoiceButton : MonoBehaviour
{
    [SerializeField] private TMP_Text choiceLabel;
    [SerializeField] private Button button;

    public void Setup(string text, Action onClicked)
    {
        if (choiceLabel != null) choiceLabel.text = text;
        if (button == null) button = GetComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClicked?.Invoke());
    }
}
