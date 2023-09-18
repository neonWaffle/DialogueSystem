using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueInfo
{
    [field: SerializeField] public string DialogueID { get; private set; }
    [field: SerializeField] public bool IsRepeatable { get; private set; }
}
