using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public Object mainMenu;
    public Object[] LevelsInOrderAscending;

    private bool levelSelectIsShowing = false;

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

    public void SwitchToLevelByIndex(int index)
    {
        SceneManager.LoadScene(LevelsInOrderAscending[index].name.ToString());
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenu.name);

        //when returning to main menu, reset level select variable because its now hidden
        levelSelectIsShowing = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ToggleLevelSelect()
    {
        if(!levelSelectIsShowing)
        {
            GameObject.Find("LevelSelector").GetComponent<UIParallelAnimation>().Play();
        }
        else
        {
            GameObject.Find("LevelSelector").GetComponent<UIFixedAnimation>().Play();
        }
    }
}
