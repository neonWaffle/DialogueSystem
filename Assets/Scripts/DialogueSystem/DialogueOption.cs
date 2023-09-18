using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueOption
{
    public DialogueLine NextLine { get; set; }
    public DialogueBranch Branch { get; set; }
    public DialogueLine RootLine { get; set; }
    public string Text { get; private set; }
    public List<DialogueCommand> Commands { get; private set; }
    public List<DialogueCondition> Conditions { get; private set; }

    public bool WasSelected { get; private set; }
    
    public DialogueOption(string text, List<DialogueCondition> conditions, List<DialogueCommand> commands, DialogueLine rootLine)
    {
        Text = text;
        Conditions = conditions;
        Commands = commands;
        RootLine = rootLine;

        WasSelected = false;
    }

    public bool IsAvailable()
    {
        foreach (var condition in Conditions)
        {
            if (!condition.Evaluate())
            {
                return false;
            }
        }
        return true;
    }

    public void Execute()
    {
        foreach (var command in Commands)
        {
            command.Execute();
        }

        WasSelected = true;
    }

    public DialogueLine GetNextAvailableLine()
    {
        if (Branch != null)
        {
            return Branch.GetNextAvailableLine();
        }

        //Skips all dummy lines
        var line = NextLine;
        while (line != null && line.Text == null && line.NextLine != null)
        {
            line = line.NextLine;
        }
        return line;
    }
}
