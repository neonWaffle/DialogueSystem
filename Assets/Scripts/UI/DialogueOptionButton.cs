using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueOptionButton : MonoBehaviour
{
    TextMeshProUGUI text;
    DialogueOption dialogueOption;

    [SerializeField] Color regularColour = Color.black;
    [SerializeField] Color previouslySelectedColour = Color.grey;

    void Awake()
    {
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void AssignOption(DialogueOption option)
    {
        dialogueOption = option;
        text.text  =DialogueManager.Instance.DialogueParser.ReplaceVariables(option.Text);
        text.color = option.WasSelected ? previouslySelectedColour : regularColour;
    }

    public void SelectOption()
    {
        DialogueManager.Instance.SelectOption(dialogueOption);
    }
}
