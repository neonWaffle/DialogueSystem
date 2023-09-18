using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DialogueCommandRegistry
{
    Dictionary<string, Delegate> commands = new Dictionary<string, Delegate>();

    public DialogueCommandRegistry()
    {
        //Adding default functions
        commands.Add("<", new Func<float, float, bool>((left, right) => left < right));
        commands.Add("<=", new Func<float, float, bool>((left, right) => left <= right));
        commands.Add(">", new Func<float, float, bool>((left, right) => left > right));
        commands.Add(">=", new Func<float, float, bool>((left, right) => left >= right));
        commands.Add("==", new Func<object, object, bool>((left, right) => left.Equals(right)));
        commands.Add("!=", new Func<object, object, bool>((left, right) => !left.Equals(right)));
        commands.Add("=", new Action<object, object>((left, right) =>
        {
            if (right is string rightStr && rightStr.StartsWith("$"))
            {
                right = DialogueManager.Instance.VariableRegistry.GetVariable(rightStr.Substring(1));
            }
            if (left is string leftStr && leftStr.StartsWith("$"))
            {
                DialogueManager.Instance.VariableRegistry.SetVariable(leftStr.Substring(1), right);
            }
        }));
    }

    public void AddCommand(string commandName, Delegate command)
    {
        commands[commandName] = command;
    }

    public bool HasCommand(string commandName)
    {
        return commands.ContainsKey(commandName);
    }

    public bool AreParametersCompatible(string commandName, params object[] args)
    {
        return commands.TryGetValue(commandName, out var command)
            && command.Method.GetParameters().Select(arg => arg.ParameterType).SequenceEqual(args.Select(arg => arg.GetType()));
    }

    public Type GetCommandReturnType(string commandName)
    {
        if (commands.TryGetValue(commandName, out var command))
        {
            return command.Method.ReturnType;
        }
        throw new Exception($"Command {commandName} hasn't been added to the registry");
    }


    public object Execute(string commandName, params object[] arguments)
    {
        if (commands.TryGetValue(commandName, out var command))
        {
            return command.DynamicInvoke(arguments);
        }
        throw new Exception($"Command {commandName} hasn't been added to the registry");
    }
}
