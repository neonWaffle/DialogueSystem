using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueLine
{
    public string Speaker { get; private set; }
    public string Text { get; private set; }
    public string Expression { get; private set; }
    public DialogueLine NextLine { get; set; }
    public List<DialogueOption> Options { get; set; }
    public DialogueBranch Branch { get; set; }
    public List<DialogueCommand> Commands { get; private set; }

    public DialogueLine(string speaker, string text, string expression, List<DialogueCommand> commands)
    {
        Speaker = speaker;
        Text = text;
        Expression = expression;
        Options = new List<DialogueOption>();
        Commands = commands;
    }

    public DialogueLine()
    {
        Options = new List<DialogueOption>();
        Commands = new List<DialogueCommand>();
    }

    public void Execute()
    {
        foreach (var command in Commands)
        {
            command.Execute();
        }
    }

    public DialogueLine GetNextAvailableLine()
    {
        if (Branch != null)
            return Branch.GetNextAvailableLine();

        //Skips all dummy lines
        var line = NextLine;
        while (line != null && line.Text == null && line.NextLine != null)
        {
            line = line.NextLine;
        }
        return line;
    }
}
