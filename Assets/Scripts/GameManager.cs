using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
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
    }
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
    public void EndRound()
    {
        OnRoundEnded?.Invoke(this, EventArgs.Empty);
    }
    public void GameOver()
    {
        Debug.Log(" Implement game over here ");
        // EndGameUIMethodMaybe();
    }
    /// <summary>
    /// Gets called from EnemySpawner when conditions are met.
    /// No enemies alive and the wave spawning is paused.
    /// </summary>
    public void SetInbetweenWavesState(bool shouldBeActive)
    {
        if (isBetweenWaves != shouldBeActive)
        {
            isBetweenWaves = shouldBeActive;

            if (inbetweenWaves != null)
                inbetweenWaves.SetActive(shouldBeActive);

            OnInbetweenWavesStateChange?.Invoke(this, shouldBeActive);
        }
    }
}