using Shop;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentRound { get; private set; }

    public bool IsGameStarted { get; private set; }

    public int currentWave;
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
    }

    private bool Application_WantsToQuit()
    {
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
        if (currentWave % 5 == 1)
            WaveUI.Instance.WaveShortMessage("Sheep are coming!");
        SetInbetweenWavesState(false);
        OnRoundStarted?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    ///  Ends current round.
    /// </summary>
    public void EndRound()
    {
        WaveUI.Instance.WaveShortMessage(RandomMessage());
        OnRoundEnded?.Invoke(this, EventArgs.Empty);
    }
    /// <summary>
    /// 
    /// </summary>
    private string RandomMessage()
    {
        System.Random random = new System.Random();
        int randChoice = random.Next(1, 5);
        switch (randChoice)
        {
            case 1:
                return "Wool done! They stood no chance.";
            case 2:
                return "Those sheep just got sheared!";
            case 3:
                return "Fluffy invasion stopped!";
            case 4:
                return "Not a single baa left alive.";
            default:
                return "Null null bug bug";
        }
    }
    /// <summary>
    /// Sets IsGameStarted to true, which allows game loop to run.
    /// </summary>
    public void StartGame()
    {
        IsGameStarted = true;
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

    #region Debug
    public void StartRound_DEBUG()
    {
        currentWave++;
        SetInbetweenWavesState(false);
        OnRoundStarted?.Invoke(this, EventArgs.Empty);
    }

    public void EndRound_DEBUG()
    {
        OnRoundEnded?.Invoke(this, EventArgs.Empty);
    }
    #endregion
}

