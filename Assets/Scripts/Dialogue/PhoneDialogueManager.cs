using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives the "Phone a Expert" conversation, including branching player choices.
///
/// Wiring:
///   - contentParent          -> Scroll View/Viewport/Content   (chat bubbles go here)
///   - playerBubblePrefab     -> your "Player" bubble prefab
///   - specialistBubblePrefab -> your "RingExpert" bubble prefab
///   - scrollRect             -> the Scroll View's ScrollRect
///   - choicesContainer       -> an empty panel (e.g. below the Scroll View) that will hold choice buttons
///   - choiceButtonPrefab     -> a Button prefab with a DialogueChoiceButton component
///
/// Call StartDialogue(dialogueData) to begin. It walks dialogueData.startNodeID onward,
/// following nextNodeID automatically, or pausing at any node with "choices" for player input.
/// </summary>
public class PhoneDialogueManager : MonoBehaviour
{
    [Header("Chat Setup")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject playerBubblePrefab;
    [SerializeField] private GameObject specialistBubblePrefab;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Choice Setup")]
    [SerializeField] private Transform choicesContainer;
    [SerializeField] private GameObject choiceButtonPrefab;

    [Header("Timing")]
    [SerializeField] private float messageDelay = 1.2f;
    [SerializeField] private float specialistThinkDelay = 0.8f;

    public event Action<string> OnInspectionHint;
    public event Action OnDialogueComplete;

    private Dictionary<string, DialogueNode> nodeLookup;
    private DialogueData currentDialogue;
    private Coroutine playRoutine;

    private bool waitingForChoice;
    private string pendingNextNodeID;

    public void StartDialogue(DialogueData dialogueData)
    {
        if (dialogueData == null || dialogueData.nodes == null || dialogueData.nodes.Length == 0)
        {
            Debug.LogWarning("PhoneDialogueManager: DialogueData is missing or has no nodes.");
            return;
        }

        if (playRoutine != null) StopCoroutine(playRoutine);

        currentDialogue = dialogueData;
        BuildNodeLookup(dialogueData);

        ClearContent();
        ClearChoices();

        playRoutine = StartCoroutine(PlayFromNode(dialogueData.startNodeID));
    }

    public void StopDialogue()
    {
        if (playRoutine != null) StopCoroutine(playRoutine);
        ClearChoices();
    }

    private void BuildNodeLookup(DialogueData dialogueData)
    {
        nodeLookup = new Dictionary<string, DialogueNode>();
        foreach (DialogueNode node in dialogueData.nodes)
        {
            if (string.IsNullOrEmpty(node.nodeID)) continue;

            if (!nodeLookup.ContainsKey(node.nodeID))
                nodeLookup.Add(node.nodeID, node);
            else
                Debug.LogWarning($"PhoneDialogueManager: duplicate nodeID '{node.nodeID}' in {dialogueData.name}");
        }
    }

    private void ClearContent()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Transform child = contentParent.GetChild(i);

            // Don't destroy the choices container if it happens to live inside contentParent
            if (choicesContainer != null && child == choicesContainer)
                continue;

            Destroy(child.gameObject);
        }
    }

    private void ClearChoices()
    {
        if (choicesContainer == null) return;
        for (int i = choicesContainer.childCount - 1; i >= 0; i--)
            Destroy(choicesContainer.GetChild(i).gameObject);
    }

    private IEnumerator PlayFromNode(string nodeID)
    {
        while (!string.IsNullOrEmpty(nodeID))
        {
            if (!nodeLookup.TryGetValue(nodeID, out DialogueNode node))
            {
                Debug.LogWarning($"PhoneDialogueManager: no node with ID '{nodeID}' found.");
                break;
            }

            bool isSpecialist = node.speaker == Speaker.Specialist;

            if (!string.IsNullOrEmpty(node.text))
            {
                float delay = messageDelay + (isSpecialist ? specialistThinkDelay : 0f);
                yield return new WaitForSeconds(delay);

                SpawnBubble(node.speaker, node.text);

                if (!string.IsNullOrEmpty(node.inspectionHintID))
                    OnInspectionHint?.Invoke(node.inspectionHintID);

                yield return ScrollToBottomNextFrame();
            }

            if (node.choices != null && node.choices.Length > 0)
            {
                yield return ShowChoicesAndWait(node.choices);
                nodeID = pendingNextNodeID;
            }
            else
            {
                nodeID = node.nextNodeID;
            }
        }

        OnDialogueComplete?.Invoke();
        playRoutine = null;
    }

    private IEnumerator ShowChoicesAndWait(DialogueChoice[] choices)
    {
        waitingForChoice = true;
        pendingNextNodeID = null;

        foreach (DialogueChoice choice in choices)
        {
            GameObject btnGO = Instantiate(choiceButtonPrefab, choicesContainer);
            DialogueChoiceButton btn = btnGO.GetComponent<DialogueChoiceButton>();

            DialogueChoice capturedChoice = choice; // avoid closure-over-loop-variable bug
            btn.Setup(choice.choiceText, () => OnChoicePicked(capturedChoice));
        }

        yield return new WaitUntil(() => !waitingForChoice);
    }

    private void OnChoicePicked(DialogueChoice choice)
    {
        if (!waitingForChoice) return; // ignore rapid double-clicks

        waitingForChoice = false;
        pendingNextNodeID = choice.nextNodeID;

        ClearChoices();

        // Echo the player's pick into the chat as their own bubble.
        SpawnBubble(Speaker.Player, choice.choiceText);
        StartCoroutine(ScrollToBottomNextFrame());
    }

    private void SpawnBubble(Speaker speaker, string text)
    {
        bool isSpecialist = speaker == Speaker.Specialist;
        GameObject prefab = isSpecialist ? specialistBubblePrefab : playerBubblePrefab;
        GameObject bubbleGO = Instantiate(prefab, contentParent);

        MessageBubble bubble = bubbleGO.GetComponent<MessageBubble>();
        if (bubble == null)
        {
            Debug.LogWarning("Bubble prefab is missing a MessageBubble component.");
            return;
        }

        string speakerLabel = isSpecialist ? currentDialogue.specialistName : "Player";
        bubble.SetMessage(speakerLabel, text);

        // Keep Replies pinned below all chat bubbles
        if (choicesContainer != null && choicesContainer.parent == contentParent)
            choicesContainer.SetAsLastSibling();
    }

    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null;
        yield return null;
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}
