using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class DialogueBranch
{
    public class BranchData
    {
        public DialogueLine NextLine { get; set; }
        public List<DialogueCondition> Conditions { get; set; }

        public BranchData(List<DialogueCondition> conditions)
        {
            Conditions = conditions;
        }
    }

    public bool HasDefaultBranch => Branches.FirstOrDefault(branch => branch.Conditions == null || branch.Conditions.Count == 0) != null;

    public List<BranchData> Branches { get; private set; }

    public DialogueBranch()
    {
        Branches = new List<BranchData>();
    }

    public void AddNewBranch(List<DialogueCondition> conditions)
    {
        Branches.Add(new BranchData(conditions));
    }

    public void AddDialogueLine(DialogueLine line)
    {
        Branches[Branches.Count - 1].NextLine = line;
    }

    public DialogueLine GetNextAvailableLine()
    {
        foreach (var branch in Branches)
        {
            bool isAvailable = true;
            
            foreach (var condition in branch.Conditions)
            {
                if (!condition.Evaluate())
                {
                    isAvailable = false;
                    break;
                }
            }
        
            if (isAvailable)
            {  
                //Skips all dummy lines
                var line = branch.NextLine;
                while (line != null && line.Text == null && line.NextLine != null)
                {
                    line = line.NextLine;
                }
                return line;
            }
        }

        throw new Exception($"DialogueBranch does not have an available DialogueLine to go to");
    }
}
