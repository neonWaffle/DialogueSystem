using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public DialogueParser DialogueParser { get; private set; }
    public DialogueActorRegistry ActorRegistry { get; private set; }
    public DialogueVariableRegistry VariableRegistry { get; private set; }
    public DialogueCommandRegistry CommandRegistry { get; private set; }

    [SerializeField] string dialogueFilePath = "Assets/StreamingAssets/Dialogues/TestDialogue.dlg";

    public Dictionary<string, DialogueLine> Dialogues = new Dictionary<string, DialogueLine>();
    DialogueLine currentLine;
    DialogueActor playerActor;

    [SerializeField] DialogueActorPortrait speakerPortrait;
    [SerializeField] DialogueActorPortrait playerPortrait;
    [SerializeField] TextMeshProUGUI dialogueText;
    [SerializeField] TextMeshProUGUI speakerNameText;

    [SerializeField] GameObject continueButton;
    [SerializeField] GameObject finishButton;
    DialogueOptionButton[] optionButtons;

    [SerializeField] float typeDelay = 0.1f;

    Canvas canvas;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        canvas = GetComponentInChildren<Canvas>();
        optionButtons = GetComponentsInChildren<DialogueOptionButton>();

        ActorRegistry = new DialogueActorRegistry();
        VariableRegistry = new DialogueVariableRegistry();
        CommandRegistry = new DialogueCommandRegistry();

        DialogueParser = new DialogueParser(VariableRegistry, CommandRegistry);

        canvas.enabled = false;
    }

    void Start()
    {
        playerActor = GameObject.FindGameObjectWithTag("Player").GetComponent<DialogueActor>();
        LoadDialogues(dialogueFilePath);
    }

    void LoadDialogues(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        Dialogues = DialogueParser.ParseLines(lines);
    }

    public void StartDialogue(string dialogueId)
    {
        GameManager.Instance.ChangeGameState(GameState.Dialogue);

        currentLine = Dialogues[dialogueId];
        if (string.IsNullOrWhiteSpace(currentLine.Text))
        {
            currentLine = currentLine.Branch.GetNextAvailableLine();
        }

        continueButton.SetActive(false);
        finishButton.SetActive(false);
        HideOptions();
        canvas.enabled = true;

        DisplayDialogue();
    }

    public void Continue()
    {
        continueButton.SetActive(false);
        currentLine = currentLine.GetNextAvailableLine();
        DisplayDialogue();
    }

    public void Finish()
    {
        canvas.enabled = false;
        finishButton.SetActive(false);
        GameManager.Instance.ChangeGameState(GameState.Gameplay);
    }

    public void SelectOption(DialogueOption option)
    {
        HideOptions();
        option.Execute();
        currentLine = option.GetNextAvailableLine();
        if (currentLine != null)
        {
            DisplayDialogue();
        }
        else
        {
            Finish();
        }
    }

    void DisplayDialogue()
    {
        if (currentLine == null || currentLine.Text == null)
        {
            Finish();
            return;
        }

        bool isPlayerTurn = currentLine.Speaker.Equals(playerActor.Name);
        if (isPlayerTurn)
        {
            playerPortrait.Setup(playerActor, currentLine.Expression);
            playerPortrait.SetTurn(true);
            speakerPortrait.SetTurn(false);
        }
        else
        {
            speakerPortrait.Setup(ActorRegistry.GetActor(currentLine.Speaker), currentLine.Expression);
            speakerPortrait.SetTurn(true);
            playerPortrait.SetTurn(false);
        }

        dialogueText.text = DialogueParser.ReplaceVariables(currentLine.Text);
        dialogueText.maxVisibleCharacters = 1;
        speakerNameText.text = currentLine.Speaker;

        StartCoroutine(TypeDialogue());
    }

    IEnumerator TypeDialogue()
    {
        while (dialogueText.maxVisibleCharacters < dialogueText.text.Length)
        {
            dialogueText.maxVisibleCharacters++;
            float t = 0.0f;
            while (t <= 1.0f)
            {
                t += Time.deltaTime / typeDelay;
                yield return null;
                if (Input.GetMouseButtonDown(0))
                {
                    FinishTyping();
                    yield break;
                }
            }
        }

        FinishTyping();
    }

    void FinishTyping()
    {
        currentLine.Execute();

        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        if (currentLine.Options != null && currentLine.Options.Count > 0)
        {
            ShowOptions();
        }
        else if (currentLine.NextLine != null || (currentLine.Branch != null && currentLine.Branch.GetNextAvailableLine() != null))
        {
            continueButton.SetActive(true);
        }
        else
        {
            finishButton.SetActive(true);
        }
    }

    void ShowOptions()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < currentLine.Options.Count && currentLine.Options[i].IsAvailable())
            {
                optionButtons[i].AssignOption(currentLine.Options[i]);
                optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    void HideOptions()
    {
        foreach (var option in optionButtons)
        {
            option.gameObject.SetActive(false);
        }
    }
}
