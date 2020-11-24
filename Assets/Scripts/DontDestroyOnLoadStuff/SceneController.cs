using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public bool isInLevel = false;
    public Object mainMenu;
    public Object[] LevelsInOrderAscending;
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
        SceneManager.LoadScene(LevelsInOrderAscending[currentLevelIndex].name.ToString());

        isInLevel = true;
    }

    public void SwitchToLevelByIndex(int index)
    {
        SpanningUIController.Instance.ResetUI();

        currentLevelIndex = index;
        SceneManager.LoadScene(LevelsInOrderAscending[index].name.ToString());

        isInLevel = true;
    }

    public void ResetCurrentLevel()
    {
        SwitchToLevelByIndex(currentLevelIndex);
    }

    public void GoToMainMenu()
    {
        SpanningUIController.Instance.ResetUI();

        SceneManager.LoadScene(mainMenu.name);

        isInLevel = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
