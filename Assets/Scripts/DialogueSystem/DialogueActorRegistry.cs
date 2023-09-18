using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueActorRegistry
{
    Dictionary<string, DialogueActor> actors = new Dictionary<string, DialogueActor>();

    public void AddActor(string name, DialogueActor actor)
    {
        actors[name] = actor;
    }

    public DialogueActor GetActor(string name)
    {
        return actors[name];
    }
}
