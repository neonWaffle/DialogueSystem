using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    CharacterController characterController;
    new Camera camera;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 3.0f;
    [SerializeField] float turnSpeed = 15.0f;
    Vector3 dir;

    [Header("Interaction")]
    [SerializeField] float interactionRange = 1.0f;
    [SerializeField] LayerMask interactionLayerMask;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        camera = Camera.main;
    }

    void Update()
    {
        if (GameManager.Instance.GameState == GameState.Dialogue)
            return;

        HandleInteraction();
        HandleMovement();
    }

    void HandleMovement()
    {
        dir = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
        dir = dir.x * camera.transform.right + dir.z * camera.transform.forward;
        dir.y = 0.0f;
        dir.Normalize();
        characterController.SimpleMove(moveSpeed * dir);

        if (dir != Vector3.zero)
        {
            var rot = Quaternion.LookRotation(dir);
            rot = Quaternion.Slerp(transform.rotation, rot, turnSpeed * Time.deltaTime);
            transform.rotation = rot;
        }
    }

    void HandleInteraction()
    {
        if (Input.GetButtonDown("Interact"))
        {
            var colliders = Physics.OverlapSphere(transform.position, interactionRange, interactionLayerMask);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out DialogueTrigger trigger))
                {
                    if (trigger.CanBeDisplayed())
                    {
                        trigger.StartDialogue();
                        break;
                    }
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
