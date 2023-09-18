using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class DialogueVariableRegistry
{
    Dictionary<string, object> variables = new Dictionary<string, object>();

    public bool HasVariable(string variable)
    {
        return variables.ContainsKey(variable);
    }

    public object GetVariable(string variable)
    {
        if (variables.TryGetValue(variable, out var result))
        {
            return result;
        }
        Assert.IsNotNull(result, $"{variable} is not present in the variable registry");
        return null;
    }

    public void SetVariable(string variableName, object variable)
    {
        variables[variableName] = variable;
    }
}
