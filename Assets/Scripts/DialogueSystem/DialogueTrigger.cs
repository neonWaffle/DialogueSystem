using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] DialogueInfo dialogueInfo;
    bool wasDisplayed = false;

    [SerializeField] float interactionRange = 2.5f;
    [SerializeField] SphereCollider rangeCollider;
    [SerializeField] GameObject interactionPopup;

    [SerializeField] float turnSpeed = 10.0f;
    GameObject player;

    void Awake()
    {
        rangeCollider.radius = interactionRange;
        TogglePopup(false);
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && CanBeDisplayed())
        {
            TogglePopup(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TogglePopup(false);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }

    public void StartDialogue()
    {
        if (CanBeDisplayed())
        {
            TogglePopup(dialogueInfo.IsRepeatable);
            wasDisplayed = true;
            DialogueManager.Instance.StartDialogue(dialogueInfo.DialogueID);
            StartCoroutine(FacePlayer());
        }
    }

    public bool CanBeDisplayed()
    {
        return dialogueInfo.IsRepeatable || !wasDisplayed;
    }

    void TogglePopup(bool isEnabled)
    {
        interactionPopup.SetActive(isEnabled);
    }

    IEnumerator FacePlayer()
    {
        var startRot = transform.rotation;
        var targRot = Quaternion.LookRotation((player.transform.position - transform.position).normalized);
        float t = 0.0f;
        while (t < 1.0f)
        {
            transform.rotation = Quaternion.Slerp(startRot, targRot, t);
            t += Time.deltaTime * turnSpeed;
            yield return null;
        }
    }
}
