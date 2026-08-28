using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue/Specialist Call")]
public class DialogueData : ScriptableObject
{
    public string specialistName;
    public int callFee;

    [Tooltip("The nodeID of the first node to show when the call starts.")]
    public string startNodeID;

    public DialogueNode[] nodes;
}

public enum Speaker { Player, Specialist }

[System.Serializable]
public class DialogueNode
{
    [Tooltip("Unique ID for this node. Referenced by 'nextNodeID' fields and choice targets.")]
    public string nodeID;

    public Speaker speaker;

    [Tooltip("Shown as a chat bubble. Leave blank if this node ONLY presents choices with no bubble of its own.")]
    [TextArea(2, 4)]
    public string text;

    [Tooltip("Optional - fires OnInspectionHint when this node's bubble is shown.")]
    public string inspectionHintID;

    [Tooltip("Node to auto-advance to after this bubble appears. Leave blank to end the call here. Ignored if 'choices' has entries.")]
    public string nextNodeID;

    [Tooltip("If filled in, the player is shown these as tappable options instead of auto-advancing. Typically used on a Player-speaker node.")]
    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)]
    public string choiceText;

    [Tooltip("nodeID to jump to when the player picks this choice.")]
    public string nextNodeID;
}
