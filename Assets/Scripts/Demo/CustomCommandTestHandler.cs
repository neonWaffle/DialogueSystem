using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CustomCommandTestHandler : MonoBehaviour
{
    int testSkill = 5;
    [SerializeField] AudioClip[] testAudioClips;
    [SerializeField] Material[] testSkyboxes;
    [SerializeField] GameObject testSFX;

    GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        DialogueManager.Instance.CommandRegistry.AddCommand("PlayAudio", new Action<string, bool>(PlayAudio));
        DialogueManager.Instance.CommandRegistry.AddCommand("PauseAudio", new Action(PauseAudio));
        DialogueManager.Instance.CommandRegistry.AddCommand("SkillCheck", new Func<int, bool>(SkillCheck));
        DialogueManager.Instance.CommandRegistry.AddCommand("IncreaseSkill", new Action<int>(IncreaseSkill));
        DialogueManager.Instance.CommandRegistry.AddCommand("RollDice", new Func<int, bool>(RollDice));
        DialogueManager.Instance.CommandRegistry.AddCommand("ChangeColour", new Action<string>(ChangePlayerColour));
        DialogueManager.Instance.CommandRegistry.AddCommand("ChangeSkybox", new Action<int>(ChangeSkybox));
        DialogueManager.Instance.CommandRegistry.AddCommand("ToggleSFX", new Action<bool>(ToggleSFX));

        ToggleSFX(false);
    }

    void PlayAudio(string audioName, bool shouldLoop)
    {
        foreach (var clip in testAudioClips)
        {
            if (clip.name.Equals(audioName))
            {
                AudioManager.Instance.PlayAudio(clip, shouldLoop);
                break;
            }
        }
    }

    void PauseAudio()
    {
        AudioManager.Instance.Pause();
    }

    bool SkillCheck(int minAmount)
    {
        return testSkill >= minAmount;
    }

    void IncreaseSkill(int amount)
    {
        testSkill += amount;
    }

    bool RollDice(int minAmount)
    {
        return UnityEngine.Random.Range(0, 20) >= minAmount;
    }

    void ChangePlayerColour(string colourStr)
    {
        if (ColorUtility.TryParseHtmlString(colourStr, out var colour))
        {
            player.GetComponent<Renderer>().material.color = colour;
        }
    }

    void ChangeSkybox(int skyboxID)
    {
        RenderSettings.skybox = testSkyboxes[skyboxID];
    }

    void ToggleSFX(bool shouldEnable)
    {
        testSFX.SetActive(shouldEnable);
    }
}
