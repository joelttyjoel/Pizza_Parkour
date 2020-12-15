using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EasyUIAnimator;
using UnityEngine.UI;

public class SpanningUIController : MonoBehaviour
{
    [Header("General stuff")]
    public string winBackgroundToPreventClicks;
    [Header("Win Screen Stuff")]
    public string winImage;
    public string winMenu;
    public string scoreText;
    public string highscoreText;
    [Header("Level selector stuff")]
    public string levelSelector;
    
    [System.NonSerialized]
    public bool levelSelectIsShowing = false;

    //singleton
    public static SpanningUIController Instance;
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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneController.Instance.isInLevel) SceneController.Instance.GoToMainMenu();
        }
    }

    public void ShowWinScreen()
    {
        //update score from clock
        GameObject.Find(scoreText).GetComponent<Text>().text = "Level Score: " + LevelClockController.Instance.currentTimeClock.ToString("F2");
        GameObject.Find(highscoreText).GetComponent<Text>().text = "Level Highscore: " + PlayerPrefs.GetFloat(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex].ToString() + "_HighScore").ToString("F2");

        //this is just hardcoded in, kinda has to be
        GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = true;
        GameObject.Find(winImage).GetComponent<UIFixedAnimation>().Play();
        GameObject.Find(winMenu).GetComponent<UIFixedAnimation>().Play();

        //doesent need to be reset because only outcome is change scene
    }

    public void ToggleLevelSelect()
    {
        if (!levelSelectIsShowing)
        {
            //update buttons
            Button[] allButtonsInOrderAscending = GameObject.Find("LevelButtons").GetComponentsInChildren<Button>();

            for (int i = 0; i < allButtonsInOrderAscending.Length; i++)
            {
                allButtonsInOrderAscending[i].interactable = GameManager.Instance.GetUnlockedStateByIndex(i);
                if(!GameManager.Instance.GetUnlockedStateByIndex(i)) allButtonsInOrderAscending[i].GetComponent<UIFixedAnimation>().Pause();
            }

            GameObject.Find(levelSelector).transform.localPosition = new Vector3(0, 0, 0);
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = true;
        }
        else
        {
            GameObject.Find(levelSelector).transform.localPosition = new Vector3(-2000, 0, 0);
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = false;
        }

        levelSelectIsShowing = !levelSelectIsShowing;
    }

    public void ResetUI()
    {
        //GameObject.Find(levelSelector).transform.position = new Vector3(9999f, 99999f, 99999f);

        GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = false;
        levelSelectIsShowing = false;
    }
}
