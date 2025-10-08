using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int currentRound { get; private set; }
    

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
    }

    public void StartRound()
    {
        currentRound++;
        OnRoundStarted?.Invoke(this, EventArgs.Empty);
    }

    public void EndRound()
    {
        OnRoundEnded?.Invoke(this, EventArgs.Empty);
    }
}
