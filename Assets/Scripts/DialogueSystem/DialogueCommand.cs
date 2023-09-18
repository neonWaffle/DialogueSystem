using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueCommand
{
    public object[] Args { get; private set; }
    public string CommandName { get; private set; }

    public DialogueCommand(string commandName, params object[] args)
    {
        CommandName = commandName;
        Args = args;
    }

    public object Execute()
    {
        object[] tempArgs = new object[Args.Length];
        Args.CopyTo(tempArgs, 0);
        for (int i = 0; i < Args.Length; i++)
        {
            if (!CommandName.Equals("=") && Args[i] is string str && str.StartsWith("$"))
            {
                tempArgs[i] = DialogueManager.Instance.VariableRegistry.GetVariable(str.Substring(1));
            }
        }
        return DialogueManager.Instance.CommandRegistry.Execute(CommandName, tempArgs);
    }
}
