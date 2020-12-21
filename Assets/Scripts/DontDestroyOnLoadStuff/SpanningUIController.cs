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
    [Header("Settings")]
    public string settingsName;
    public string sliderMusicName;
    public string sliderSoundName;
    
    [System.NonSerialized]
    public bool levelSelectIsShowing = false;
    [System.NonSerialized]
    public bool settingsIsShowing = false;

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
        if(Input.GetButtonDown("Cancel"))
        {
            if (SceneController.Instance.isInLevel) SceneController.Instance.GoToMainMenu();
        }
    }

    public void ShowWinScreen()
    {
        //yes but have to to avoid enabled menu clicking stuff
        GameObject.Find("WinScreen").transform.Find(winImage).gameObject.SetActive(true);
        GameObject.Find("WinScreen").transform.Find(winMenu).gameObject.SetActive(true);
        //foreach (GameObject a in allObjects)
        //{
        //    if (a.name == winImage)
        //    {
        //        a.SetActive(true);
        //    }
        //    else if(a.name == winMenu)
        //    {
        //        a.SetActive(true);
        //    }
        //}

        //update score from clock
        GameObject.Find(scoreText).GetComponent<Text>().text = "Level Score: " + LevelClockController.Instance.currentTimeClock.ToString("F2");
        GameObject.Find(highscoreText).GetComponent<Text>().text = "Level Highscore: " + PlayerPrefs.GetFloat(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex].ToString() + "_HighScore").ToString("F2");

        //this is just hardcoded in, kinda has to be
        GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = true;
        GameObject.Find(winImage).GetComponent<UIFixedAnimation>().Play();
        GameObject.Find(winMenu).GetComponent<UIFixedAnimation>().Play();

        //doesent need to be reset because only outcome is change scene
    }

    public void OnLevelStart()
    {
        StartCoroutine(OnLevelStartWaitForStart());
    }

    private IEnumerator OnLevelStartWaitForStart()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        GameObject.Find(winImage).SetActive(false);
        GameObject.Find(winMenu).SetActive(false);
    }

    public void ToggleLevelSelect()
    {
        if (!levelSelectIsShowing)
        {
            //Find and enable levelselect
            GameObject.Find("Canvas").transform.Find(levelSelector).gameObject.SetActive(true);
            //GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            //foreach (GameObject a in allObjects)
            //{
            //    if (a.name == levelSelector)
            //    {
            //        a.SetActive(true);
            //        break;
            //    }
            //}

            //update buttons
            Button[] allButtonsInOrderAscending = GameObject.Find("LevelButtons").GetComponentsInChildren<Button>();

            for (int i = 0; i < allButtonsInOrderAscending.Length; i++)
            {
                allButtonsInOrderAscending[i].interactable = GameManager.Instance.GetUnlockedStateByIndex(i);
                //if (!GameManager.Instance.GetUnlockedStateByIndex(i)) allButtonsInOrderAscending[i].GetComponent<UIFixedAnimation>().Pause();
            }

            GameObject.Find(levelSelector).transform.localPosition = new Vector3(0, 0, -1);
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = true;
        }
        else
        {
            GameObject.Find(levelSelector).transform.localPosition = new Vector3(-2000, 0, 0);
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = false;

            //disable levelselect
            GameObject.Find(levelSelector).SetActive(false);
        }

        levelSelectIsShowing = !levelSelectIsShowing;
    }

    public void ToggleSettingsMenu()
    {
        if (!settingsIsShowing)
        {
            //find and enable settings
            GameObject.Find("Canvas").transform.Find(settingsName).gameObject.SetActive(true);
            //GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            //foreach (GameObject a in allObjects)
            //{
            //    if (a.name == settingsName)
            //    {
            //        a.SetActive(true);
            //        break;
            //    }
            //}

            GameObject.Find(settingsName).transform.localPosition = new Vector3(0, 0, -1);
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = true;

            //GameManager.Instance.UpdateSettingsByStoredValues();
        }
        else
        {
            GameObject.Find(settingsName).transform.localPosition = new Vector3(-2000, 0, 0);
            GameObject.Find(winBackgroundToPreventClicks).GetComponent<Image>().enabled = false;

            //disable settings menu
            GameObject.Find(settingsName).SetActive(false);
        }

        settingsIsShowing = !settingsIsShowing;
    }

    public void ResetUI()
    {
        settingsIsShowing = false;
        levelSelectIsShowing = false;
    }

    //public void UpdateSettingsValues()
    //{
    //    GameObject.Find(sliderMusicName).GetComponent<SliderStuff>().UpdateValues();
    //    GameObject.Find(sliderSoundName).GetComponent<SliderStuff>().UpdateValues();
    //}

    //public void SetSettingsToSaved(float volumeMusic, float volumeSound)
    //{
    //    GameObject.Find(sliderMusicName).GetComponent<SliderStuff>().SetValues(volumeMusic);
    //    GameObject.Find(sliderSoundName).GetComponent<SliderStuff>().SetValues(volumeSound);
    //}
}
