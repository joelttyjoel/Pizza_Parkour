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

            GameObject.Find(levelSelector).GetComponent<UIParallelAnimation>().Play();
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = true;
        }
        else
        {
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = false;
            GameObject.Find(levelSelector).GetComponent<UIFixedAnimation>().Play();
        }

        levelSelectIsShowing = !levelSelectIsShowing;
    }

    public void ResetUI()
    {
        GameObject.Find(levelSelector).transform.position = new Vector3(9999f, 99999f, 99999f);

        GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = false;
        levelSelectIsShowing = false;
    }
}
