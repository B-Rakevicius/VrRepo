using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentRound { get; private set; }

    public bool IsGameStarted { get; private set; }
    
    public event EventHandler OnRoundStarted;
    
    public event EventHandler OnRoundEnded;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("There is more than one GameManager instance!");
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        
        // Event to trigger a confirmation window when player tries to exit the game.
        Application.wantsToQuit += Application_WantsToQuit;
    }

    private void Update()
    {
        // TODO: Game loop goes here. When the player press Start Game, it will update m_isGameStarted to true.
    }

    private bool Application_WantsToQuit()
    {
        // For now simply just quit. Later implement a dialog box to confirm application exit.
        Debug.Log("Quitting...");
        return true;
    }

    /// <summary>
    /// Starts the next round of the game.
    /// </summary>
    public void StartRound()
    {
        currentRound++;
        OnRoundStarted?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///  Ends current round.
    /// </summary>
    public void EndRound()
    {
        OnRoundEnded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets IsGameStarted to true, which allows game loop to run.
    /// </summary>
    public void StartGame()
    {
        IsGameStarted = true;
    }
}
