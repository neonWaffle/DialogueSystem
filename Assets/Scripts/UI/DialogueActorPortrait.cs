using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueActorPortrait : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] Color inactiveColour;
    [SerializeField] Color activeColour;

    public void Setup(DialogueActor dialogueActor, string expression)
    {
        image.sprite = dialogueActor.GetExpressionSprite(expression);
    }

    public void SetTurn(bool isCurrentSpeaker)
    {
        image.color = isCurrentSpeaker ? activeColour : inactiveColour;
    }
}
