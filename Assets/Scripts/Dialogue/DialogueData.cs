using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Specialist Call")]
public class DialogueData : ScriptableObject
{
    public string specialistName;
    public int callFee;
    public DialogueLine[] lines;
}

[System.Serializable]
public class DialogueLine
{
    public enum Speaker { Player, Specialist }
    public Speaker speaker;
    [TextArea(2, 4)]
    public string text;

    public string inspectionHintID;
}
