using UnityEngine;

/// <summary>
/// Attach this script to any 3D object (Phone, Clipboard, Machine, Item, etc.)
/// and give it a description. During the tutorial, hovering over it will display this text.
/// </summary>
public class TutorialHoverItem : MonoBehaviour
{
    [TextArea]
    [Tooltip("The text to display at the bottom of the screen when hovering over this object.")]
    public string descriptionText = "Explanation goes here.";
}
