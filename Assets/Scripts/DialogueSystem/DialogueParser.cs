using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System;
using System.Linq;

public class DialogueParser
{
    DialogueVariableRegistry variableRegistry;
    DialogueCommandRegistry commandRegistry;

    enum LineType { DialogueLine, DialogueOption, DialogueDeclaration, DialogueBranch, None }

    public DialogueParser(DialogueVariableRegistry variableRegistry, DialogueCommandRegistry commandRegistry)
    {
        this.variableRegistry = variableRegistry;
        this.commandRegistry = commandRegistry;
    }

    public Dictionary<string, DialogueLine> ParseLines(string[] lines)
    {
        var dialogues = new Dictionary<string, DialogueLine>();

        if (lines.Length == 0)
            return dialogues;

        RegisterVariables(ref lines);

        var lineStack = new Stack<DialogueLine>();
        var lastLineStack = new Stack<DialogueLine>(); //Used for nested DialogueLines
        var optionStack = new Stack<DialogueOption>();
        var branchStack = new Stack<DialogueBranch>();
        var indentMap = new Dictionary<object, int>();

        string currentDialogueTitle = string.Empty;

        var previousLineType = LineType.None;

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            RemoveComments(ref line);

            int indent = GetIndentLevel(line);
            line = line.Replace("\t", "");
            if (string.IsNullOrWhiteSpace(line))
                continue;

            EscapeDialogueBranches(indent);

            //Dialogue start
            if (line[0] == '-')
            {
                currentDialogueTitle = line.Substring(1).Trim();
                if (dialogues.ContainsKey(currentDialogueTitle))
                {
                    throw new Exception($"Dialogue {currentDialogueTitle} already exists");
                }

                lineStack.Clear();
                lastLineStack.Clear();
                optionStack.Clear();
                branchStack.Clear();
                indentMap.Clear();

                previousLineType = LineType.DialogueDeclaration;
            }
            //Dialogue option
            else if (line[0] == '>')
            {
                var commands = ParseCommands(ref line);
                var conditions = ParseConditions(ref line);
                string text = line.Substring(1).Trim();

                FindLastDialogueLines(indent);

                if (previousLineType != LineType.DialogueLine && previousLineType != LineType.DialogueOption)
                {
                    throw new Exception($"A DialogueLine or a DialogueOption is missing before a DialogueOption on line {i + 1}");
                }

                var previousDialogueLine = lineStack.Peek();
                if (indentMap[previousDialogueLine] != indent)
                {
                    throw new Exception($"DialogueLine before the DialogueOption on line {i + 1} is not indented correctly");
                }

                var dialogueOption = new DialogueOption(text, conditions, commands, previousDialogueLine);
                previousDialogueLine.Options.Add(dialogueOption);

                //If the previous DialogueOption had no DialogueLine after it, make a dummy one
                if (previousLineType == LineType.DialogueOption)
                {
                    var previousOption = optionStack.Pop();
                    var dialogueLine = new DialogueLine();
                    previousOption.NextLine = dialogueLine;
                    indentMap[dialogueLine] = indentMap[previousOption] + 1;
                    lastLineStack.Push(dialogueLine);
                }

                EscapeDialogueOptions(indent);

                //Checks if this DialogueOption should end the dialogue. No need to add it to the option stack if that's the case
                var match = Regex.Match(line, @"\[{2}end\]{2}$");
                if (!match.Success)
                {
                    indentMap[dialogueOption] = indent;
                    optionStack.Push(dialogueOption);

                    previousLineType = LineType.DialogueOption;
                }
            }
            //Dialogue branch
            else if (line.StartsWith("(if"))
            {
                if (dialogues.Count == 0 && previousLineType != LineType.DialogueDeclaration)
                {
                    throw new Exception($"Dialogue declaration is missing before line {i + 1}");
                }

                if (previousLineType != LineType.DialogueDeclaration && previousLineType != LineType.DialogueLine && previousLineType != LineType.DialogueOption)
                {
                    throw new Exception($"A DialogueLine or a DialogueOption is missing before the DialogueBranch on line {i + 1}");
                }

                if ((previousLineType == LineType.DialogueLine && indent != indentMap[lineStack.Peek()])
                    || (previousLineType == LineType.DialogueOption && indent - indentMap[optionStack.Peek()] != 1)
                    || (previousLineType == LineType.DialogueDeclaration && indent > 0))
                {
                    throw new Exception($"DialogueBranch on line {i + 1} is not indented correctly");
                }

                var branch = new DialogueBranch();
                indentMap[branch] = indent;
                var conditions = ParseConditions(ref line);
                branch.AddNewBranch(conditions);
                branchStack.Push(branch);

                //If this happens to be the first line in the dialogue, make a dummy DialogueLine
                if (previousLineType == LineType.DialogueDeclaration)
                {
                    var dialogueLine = new DialogueLine();
                    dialogueLine.Branch = branch;
                    dialogues.Add(currentDialogueTitle, dialogueLine);
                }
                else if (previousLineType == LineType.DialogueOption)
                {
                    optionStack.Peek().Branch = branch;
                }
                else if (previousLineType == LineType.DialogueLine)
                {
                    lineStack.Peek().Branch = branch;
                }

                FindLastDialogueLines(indent);
                EscapeDialogueOptions(indent);

                previousLineType = LineType.DialogueBranch;
            }

            //Dialogue branch
            else if (line.StartsWith("(else"))
            {
                if (branchStack.Count == 0 || indentMap[branchStack.Peek()] != indent)
                {
                    throw new Exception($"A DialogueBranch on line {i + 1} is missing an if statement on its indent level");
                }

                if (previousLineType != LineType.DialogueLine && previousLineType != LineType.DialogueOption)
                {
                    throw new Exception($"A DialogueLine or a DialogueOption is missing before a DialogueBranch on line {i + 1}");
                }

                var branch = branchStack.Peek();
                var conditions = ParseConditions(ref line);
                branch.AddNewBranch(conditions);

                FindLastDialogueLines(indent);
                EscapeDialogueOptions(indent);

                previousLineType = LineType.DialogueBranch;
            }
            //Dialogue line
            else
            {
                if (dialogues.Count == 0 && previousLineType != LineType.DialogueDeclaration)
                {
                    throw new Exception($"Dialogue declaration is missing before line {i + 1}");
                }

                //If the previous DialogueOption had no DialogueLine after it
                if (previousLineType == LineType.DialogueOption && indentMap[optionStack.Peek()] >= indent)
                {
                    var previousOption = optionStack.Peek();
                    var previousDialogueLine = new DialogueLine();
                    previousOption.NextLine = previousDialogueLine;
                    indentMap[previousDialogueLine] = indentMap[previousOption] + 1;
                    lastLineStack.Push(previousDialogueLine);
                    previousLineType = LineType.DialogueLine;
                }

                if (previousLineType == LineType.DialogueLine && indentMap[lineStack.Peek()] != indent
                    && ((branchStack.Count == 0 && optionStack.Count == 0)
                    || ((branchStack.Count == 0 || Mathf.Abs(indent - indentMap[branchStack.Peek()]) > 1)
                    && (optionStack.Count == 0 || Mathf.Abs(indent - indentMap[optionStack.Peek()]) > 1))))
                {
                    throw new Exception($"A DialogueLine on line {i + 1} is not indented correctly");
                }
                else if (previousLineType == LineType.DialogueBranch && indent - indentMap[branchStack.Peek()] != 1)
                {
                    throw new Exception($"A DialogueLine on line {i + 1} is not indented correctly");
                }
                else if (previousLineType == LineType.DialogueOption && indent - indentMap[optionStack.Peek()] != 1)
                {
                    throw new Exception($"A DialogueLine on line {i + 1} is not indented correctly");
                }

                if (branchStack.Count > 0 && indentMap[branchStack.Peek()] == indent)
                {
                    var previousBranch = branchStack.Pop();
                    if (!previousBranch.HasDefaultBranch)
                    {
                        HandleDeadEndDialogueBranch(previousBranch);
                    }
                }

                var commands = ParseCommands(ref line);
                string expression = ParseExpression(ref line);

                bool isReturningToPreviousOptionLine = false;
                int returnDepth = 0;
                var matches = Regex.Matches(line, @"<-");
                if (matches.Count > 0)
                {
                    line = line.Replace("<-", string.Empty).Trim();
                    isReturningToPreviousOptionLine = true;
                    returnDepth = matches.Count;
                }

                string[] components = line.Split(new char[] { ':' }, 2);
                if (components.Length != 2 || string.IsNullOrWhiteSpace(components[0]) || string.IsNullOrWhiteSpace(components[0]))
                {
                    throw new Exception($"DialogueLine on line {i + 1} is not formatted correctly");
                }

                string name = components[0].Trim();
                string text = components[1].Trim();
                text = text.Replace("\\n", "\n");
                var dialogueLine = new DialogueLine(name, text, expression, commands);
                indentMap[dialogueLine] = indent;

                if (previousLineType == LineType.DialogueDeclaration)
                {
                    dialogues.Add(currentDialogueTitle, dialogueLine);
                }
                else if (previousLineType == LineType.DialogueLine)
                {
                    lineStack.Pop().NextLine = dialogueLine;
                    while (lastLineStack.Count > 0 && indentMap[lastLineStack.Peek()] >= indent)
                    {
                        lastLineStack.Pop().NextLine = dialogueLine;
                    }
                }
                else if (previousLineType == LineType.DialogueOption)
                {
                    optionStack.Peek().NextLine = dialogueLine;
                }
                else if (previousLineType == LineType.DialogueBranch)
                {
                    branchStack.Peek().AddDialogueLine(dialogueLine);
                }

                FindLastDialogueLines(indent);
                EscapeDialogueOptions(indent);

                //Handles returning to the DialogueLine that is the root of a DialogueOption that this line was a part of
                if (isReturningToPreviousOptionLine)
                {
                    var tempStack = new Stack<DialogueOption>(optionStack.Reverse());
                    while (returnDepth > 1 && tempStack.Count > 0)
                    {
                        tempStack.Pop();
                        returnDepth--;
                    }

                    if (tempStack.Count == 0 || returnDepth > 1)
                    {
                        throw new Exception($"DialogueOption on line {i + 1} is not nested deep enough to return so many times");
                    }

                    dialogueLine.NextLine = tempStack.Pop().RootLine;
                }
                else
                {
                    lineStack.Push(dialogueLine);
                }

                //Handles all the previous DialogueLines that should be leading to this one
                while (lastLineStack.Count > 0 && indentMap[lastLineStack.Peek()] > indent
                    && (optionStack.Count == 0 || indentMap[optionStack.Peek()] >= indent
                    || (indentMap[optionStack.Peek()] >= indentMap[lastLineStack.Peek()])))
                {
                    lastLineStack.Pop().NextLine = dialogueLine;
                }

                previousLineType = LineType.DialogueLine;
            }
        }

        void HandleDeadEndDialogueBranch(DialogueBranch previousBranch)
        {
            var dummyDialogueLine = new DialogueLine();
            previousBranch.AddNewBranch(new List<DialogueCondition>());
            previousBranch.AddDialogueLine(dummyDialogueLine);
            lineStack.Push(dummyDialogueLine);
            indentMap[dummyDialogueLine] = indentMap[previousBranch] + 1;
            previousLineType = LineType.DialogueLine;

            EscapeDialogueOptions(indentMap[previousBranch] + 1);
        }

        void FindLastDialogueLines(int indent)
        {
            while (lineStack.Count > 0 && indent < indentMap[lineStack.Peek()])
            {
                lastLineStack.Push(lineStack.Pop());
            }
        }

        void EscapeDialogueOptions(int indent)
        {
            while (optionStack.Count > 0 && indent <= indentMap[optionStack.Peek()])
            {
                optionStack.Pop();
            }
        }

        void EscapeDialogueBranches(int indent)
        {
            while (branchStack.Count > 0 && indent < indentMap[branchStack.Peek()])
            {
                var escapedBranch = branchStack.Pop();
                if (!escapedBranch.HasDefaultBranch)
                {
                    HandleDeadEndDialogueBranch(escapedBranch);
                }
            }
        }

        if (dialogues.Count == 0)
        {
            throw new Exception($"No dialogue has been added");
        }

        return dialogues;
    }

    public string ReplaceVariables(string line)
    {
        string pattern = @"({\$([^{}]*)})";
        var match = Regex.Match(line, pattern);
        while (match.Success)
        {
            object variable = variableRegistry.GetVariable(match.Groups[2].Value);
            line = line.Replace(match.Groups[1].Value, variable.ToString());
            match = Regex.Match(line, pattern);
        }
        return line;
    }

    int GetIndentLevel(string line)
    {
        int indent = 0;
        for (int j = 0; j < line.Length; j++)
        {
            if (line[j] == '\t')
            {
                indent++;
            }
            else
            {
                break;
            }
        }
        return indent;
    }

    string ParseExpression(ref string line)
    {
        string expression = "neutral";
        var match = Regex.Match(line, @"\[{2}(.*?)\]{2}");
        if (match.Success)
        {
            expression = match.Groups[1].Value;
            line = line.Remove(match.Groups[0].Index, match.Groups[0].Length);
        }
        return expression;
    }

    List<DialogueCondition> ParseConditions(ref string line)
    {
        var conditions = new List<DialogueCondition>();
        var match = Regex.Match(line, @"\((?:else\s)?if\s?(.*)\)");
        if (match.Success)
        {
            string[] functions = match.Groups[1].Value.Split(new string[] { "and" }, StringSplitOptions.None);
            foreach (var function in functions)
            {
                bool isValid = false;
                var conditionMatch = Regex.Match(function, @"([\w$]+)\s?([=!<>]{2})\s?([\w""$]+)");
                if (conditionMatch.Success)
                {
                    object leftOp = ParseVariable(conditionMatch.Groups[1].Value);
                    object rightOp = ParseVariable(conditionMatch.Groups[3].Value);
                    string functionName = conditionMatch.Groups[2].Value;
                    conditions.Add(new DialogueCondition(new DialogueCommand(functionName, leftOp, rightOp)));

                    isValid = true;
                }
                else
                {
                    var command = ParseCommand(function);
                    if (command != null)
                    {
                        var returnType = commandRegistry.GetCommandReturnType(command.CommandName);
                        if (returnType != typeof(bool))
                        {
                            throw new Exception($"Condition {command.CommandName} has the wrong return type ({returnType})");
                        }

                        conditions.Add(new DialogueCondition(command));
                        isValid = true;
                    }
                }

                if (!isValid)
                {
                    throw new Exception($"\"{function}\" is not a valid condition");
                }
            }

            line = line.Remove(match.Index, match.Length);
        }
        return conditions;
    }

    List<DialogueCommand> ParseCommands(ref string line)
    {
        var dialogueCommands = new List<DialogueCommand>();
        var match = Regex.Match(line, @"\((.*)\)$");
        if (match.Success)
        {
            line = line.Remove(match.Index, match.Length);
            string[] commands = match.Groups[1].Value.Split(';');
            foreach (var command in commands)
            {
                if (!string.IsNullOrWhiteSpace(command))
                {
                    dialogueCommands.Add(ParseCommand(command));
                }
            }
        }
        return dialogueCommands;
    }

    DialogueCommand ParseCommand(string command)
    {
        var match = Regex.Match(command, @"(\w+)\((.*)\)");
        if (match.Success)
        {
            string commandName = match.Groups[1].Value;
            var args = new List<object>();

            if (!string.IsNullOrWhiteSpace(match.Groups[2].Value))
            {
                string[] stringArgs = match.Groups[2].Value.Split(',');
                args = new List<object>(stringArgs.Length);
                for (int i = 0; i < stringArgs.Length; i++)
                {
                    args.Add(ParseVariable(stringArgs[i]));
                }
            }

            if (!commandRegistry.HasCommand(commandName) || !commandRegistry.AreParametersCompatible(commandName, args.ToArray()))
            {
                throw new Exception($"Command {commandName}({string.Join(",", args.Select(arg => $"{arg.GetType()}"))}) has not been added to the CommandRegistry");
            }

            return new DialogueCommand(commandName, args.ToArray());
        }
        else
        {
            match = Regex.Match(command, @"([\w$]+)\s?(=)\s?("".*""|[\w\.$""]+)");
            if (match.Success)
            {
                object leftOp = ParseVariable(match.Groups[1].Value);
                object rightOp = ParseVariable(match.Groups[3].Value);
                string functionName = match.Groups[2].Value;
                return new DialogueCommand(functionName, leftOp, rightOp);
            }
        }

        throw new Exception($"\"{command}\" is not a valid command");
    }

    object ParseVariable(string var)
    {
        if (int.TryParse(var, out int intRes))
        {
            return intRes;
        }

        if (float.TryParse(var, out float floatRes))
        {
            return floatRes;
        }

        if (bool.TryParse(var, out bool boolRes))
        {
            return boolRes;
        }

        var = var.Replace("\"", "");
        if (var.StartsWith("$") && !variableRegistry.HasVariable(var.Substring(1)))
        {
            throw new Exception($"Variable {var} has not been added to the VariableRegistry");
        }
        return var;
    }

    void RemoveComments(ref string line)
    {
        int id = line.IndexOf("//");
        if (id != -1)
        {
            line = line.Substring(0, id);
        }
    }

    void RegisterVariables(ref string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var match = Regex.Match(lines[i], @"^\$(\w*)\s?=\s?("".*""|[\w\.]+)");
            if (match.Success)
            {
                variableRegistry.SetVariable(match.Groups[1].Value, ParseVariable(match.Groups[2].Value));
                lines[i] = string.Empty;
            }
        }
    }
}
