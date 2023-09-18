using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Expression
{
    [field: SerializeField] public string Tag { get; private set; }
    [field: SerializeField] public Sprite Icon { get; private set; }
}

public class DialogueActor : MonoBehaviour
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Expression[] Expressions { get; private set; }

    void Start()
    {
        DialogueManager.Instance.ActorRegistry.AddActor(Name, this);
    }

    public Sprite GetExpressionSprite(string tag)
    {
        foreach(var expression in Expressions)
        {
            if (expression.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase))
            {
                return expression.Icon;
            }
        }
        return null;
    }
}
