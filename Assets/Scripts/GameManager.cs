using Shop;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentRound { get; private set; }

    public bool IsGameStarted { get; private set; }
    
    public int currentWave { get; private set; }
    public event EventHandler OnRoundStarted;
    public event EventHandler OnRoundEnded;
    public event EventHandler<bool> OnInbetweenWavesStateChange;
    public GameObject inbetweenWaves;
    private bool isBetweenWaves = false;
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
    private void Start()
    {
        if (inbetweenWaves != null)
            inbetweenWaves.SetActive(false);
    }
    public void StartRound()
    {
        currentWave++;
        SetInbetweenWavesState(false);
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

        // For now, instantly start first round
        StartRound();
    }
    public void GameOver()
    {
        Debug.Log(" Implement game over here ");
        // send call to UI Manager here to end game
    }
    /// <summary>
    /// Gets called from EnemySpawner when conditions are met.
    /// No enemies alive and the wave counter is a multiple of 5 ( 5/10/15/...)
    /// Spawns the "start next waves" button and should spawn shop in
    /// </summary>
    public void SetInbetweenWavesState(bool shouldBeActive)
    {
        if (isBetweenWaves != shouldBeActive)
        {
            isBetweenWaves = shouldBeActive;

            if (inbetweenWaves != null)
            {
                inbetweenWaves.SetActive(shouldBeActive);
                EndRound();
            }
            
            OnInbetweenWavesStateChange?.Invoke(this, shouldBeActive);
        }
    }
}

