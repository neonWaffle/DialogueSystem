using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    CinemachineFreeLook freeLookCamera;
    float xSpeed;
    float ySpeed;

    void Awake()
    {
        freeLookCamera = GetComponentInChildren<CinemachineFreeLook>();
        xSpeed = freeLookCamera.m_XAxis.m_MaxSpeed;
        ySpeed = freeLookCamera.m_YAxis.m_MaxSpeed;
    }

    void Start()
    {
        GameManager.Instance.OnGameStateChanged += ToggleCamera;
        ToggleCamera();
    }

    void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= ToggleCamera;
    }

    void ToggleCamera()
    {
        if (GameManager.Instance.GameState == GameState.Gameplay)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = xSpeed;
            freeLookCamera.m_YAxis.m_MaxSpeed = ySpeed;
        }
        else if (GameManager.Instance.GameState == GameState.Dialogue)
        {
            freeLookCamera.m_XAxis.m_MaxSpeed = 0.0f;
            freeLookCamera.m_YAxis.m_MaxSpeed = 0.0f;
        }
    }
}
