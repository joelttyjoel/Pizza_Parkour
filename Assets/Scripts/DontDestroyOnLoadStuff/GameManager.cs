using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    [Header("God, controls and keeps track of stuff")]
    public bool resetPlayerPrefsOnStart = false;
    public AudioMixer soundMixer;
    public AudioMixer musicMixer;

    //singleton
    public static GameManager Instance;
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

    private void Start()
    {
        //if not created, create playerprefs based on current levels (or if want to reset)
        if (PlayerPrefs.GetInt("HasPlayedBefore" + Application.version.ToString()) == 0 || resetPlayerPrefsOnStart)
        {
            PlayerPrefs.SetInt("HasPlayedBefore", 1);

            ResetMemory();
        }

        //always update mixer values from sliders
        soundMixer.SetFloat("Master", PlayerPrefs.GetFloat("SoundVolume"));
        musicMixer.SetFloat("Master", PlayerPrefs.GetFloat("MusicVolume"));

        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (SceneController.Instance.isInLevel) SceneController.Instance.GoToMainMenu();
        }
        else if (Input.GetButtonDown("ResetLevelButton"))
        {
            if (SceneController.Instance.isInLevel) SceneController.Instance.ResetCurrentLevel();
        }
    }

    private void ResetMemory()
    {
        for (int i = 0; i < SceneController.Instance.LevelsInOrderAscending.Length; i++)
        {
            //levels cleared
            //set all to 0
            PlayerPrefs.SetInt(SceneController.Instance.LevelsInOrderAscending[i], 0);
            //set first to 1, is because first level is unlocked by default
            PlayerPrefs.SetInt(SceneController.Instance.LevelsInOrderAscending[0], 1);
            //levels highscore
            //set all to shit scores
            PlayerPrefs.SetFloat(SceneController.Instance.LevelsInOrderAscending[i] + "_HighScore", 0f);
        }

        PlayerPrefs.SetFloat("MusicVolume", -15f);
        PlayerPrefs.SetFloat("SoundVolume", -10f);

        PlayerPrefs.Save();
    }

    public void UnlockNextLevel()
    {
        //if isnt last level
        if ((SceneController.Instance.currentLevelIndex + 1) < SceneController.Instance.LevelsInOrderAscending.Length)
        {
            Debug.Log("unlock: " + SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex + 1]);
            PlayerPrefs.SetInt(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex + 1], 1);
        }

        PlayerPrefs.Save();
    }

    public void CheckCurrentScoreHighscore()
    {
        //if is 0, then is first time setting score, then not new highscore just update score
        if (PlayerPrefs.GetFloat(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex] + "_HighScore") == 0f)
        {
            Debug.Log("New Score Added");
            PlayerPrefs.SetFloat(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex] + "_HighScore", LevelClockController.Instance.currentTimeClock);
        }
        //if not 0, then lets go compare
        else if (LevelClockController.Instance.currentTimeClock < PlayerPrefs.GetFloat(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex] + "_HighScore"))
        {
            Debug.Log("New HighScore!!");
            PlayerPrefs.SetFloat(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex] + "_HighScore", LevelClockController.Instance.currentTimeClock);
        }

        PlayerPrefs.Save();
    }

    public bool GetUnlockedStateByIndex(int index)
    {
        if(PlayerPrefs.GetInt(SceneController.Instance.LevelsInOrderAscending[index]) == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //public void UpdateSettingsByStoredValues()
    //{
    //    SpanningUIController.Instance.SetSettingsToSaved(PlayerPrefs.GetFloat("MusicVolume"), PlayerPrefs.GetFloat("SoundVolume"));
    //}

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
