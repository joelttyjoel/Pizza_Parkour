using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public bool isInLevel = false;
    public string mainMenuNamn;
    public string[] LevelsInOrderAscending;
    public int currentLevelIndex = 0;

    //singleton
    public static SceneController Instance;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    public void SwitchToNextLevel()
    {
        SpanningUIController.Instance.ResetUI();

        currentLevelIndex++;
        SceneManager.LoadScene(LevelsInOrderAscending[currentLevelIndex]);

        SpanningUIController.Instance.OnLevelStart();

        isInLevel = true;
    }

    public void SwitchToLevelByIndex(int index)
    {
        SpanningUIController.Instance.ResetUI();

        currentLevelIndex = index;
        SceneManager.LoadScene(LevelsInOrderAscending[index]);

        SpanningUIController.Instance.OnLevelStart();

        isInLevel = true;
    }

    public void ResetCurrentLevel()
    {
        SwitchToLevelByIndex(currentLevelIndex);
    }

    public void GoToMainMenu()
    {
        SpanningUIController.Instance.ResetUI();

        SceneManager.LoadScene(mainMenuNamn);

        isInLevel = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
