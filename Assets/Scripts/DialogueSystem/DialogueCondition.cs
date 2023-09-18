using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueCondition
{
    public DialogueCommand Command { get; private set; }

    public DialogueCondition(DialogueCommand command)
    {
        Command = command;
    }

    public bool Evaluate()
    {
        return (bool)Command.Execute();
    }
}
