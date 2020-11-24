using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationManager : MonoBehaviour
{
    [Header("Add all destinations needed here")]
    public GameObject[] allDestinations;
    [Header("Dont touch down here ok, just for showing")]
    public int completedDestinationsCount;
    public bool[] completedDestinations;

    //singleton
    public static DestinationManager Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        completedDestinations = new bool[allDestinations.Length];
    }

    public void SetDestinationCompleted(GameObject destinationCompleted)
    {
        completedDestinationsCount++;
        for (int i = 0; i < allDestinations.Length; i++)
        {
            if(allDestinations[i].GetInstanceID() == destinationCompleted.GetInstanceID())
            {
                completedDestinations[i] = true;
            }
        }

        CheckCompleteAllDestinations();
    }

    public void SetDestinationFailed(GameObject destinationCompleted)
    {
        completedDestinationsCount--;
        for (int i = 0; i < allDestinations.Length; i++)
        {
            if (allDestinations[i].GetInstanceID() == destinationCompleted.GetInstanceID())
            {
                completedDestinations[i] = false;
            }
        }

        CheckCompleteAllDestinations();
    }

    public void CheckCompleteAllDestinations()
    {
        if (completedDestinationsCount == allDestinations.Length)
        {
            LevelClockController.Instance.runClock = false;
            GameManager.Instance.UnlockNextLevel();
            GameManager.Instance.CheckCurrentScoreHighscore();

            SpanningUIController.Instance.ShowWinScreen();
        }
    }
}
