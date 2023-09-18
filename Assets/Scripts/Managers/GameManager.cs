using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum GameState { Gameplay, Dialogue }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState GameState { get; private set; }
    public event Action OnGameStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        ChangeGameState(GameState.Gameplay);
    }

    public void ChangeGameState(GameState state)
    {
        GameState = state;
        if (state == GameState.Gameplay)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else if (state == GameState.Dialogue)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        OnGameStateChanged?.Invoke();
    }
}
