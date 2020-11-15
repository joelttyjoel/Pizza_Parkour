using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("God, controls and keeps track of stuff")]
    public bool resetPlayerPrefsOnStart = false;
    public bool showWinScreen = false;

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

    private void Update()
    {
        if(showWinScreen)
        {
            showWinScreen = false;
            SpanningUIController.Instance.ShowWinScreen();
        }
    }

    private void Start()
    {
        //if not created, create playerprefs based on current levels (or if want to reset)
        if(PlayerPrefs.GetInt("HasPlayedBefore") == 0 || resetPlayerPrefsOnStart)
        {
            PlayerPrefs.SetInt("HasPlayedBefore", 1);

            ResetMemory();
        }
    }

    private void ResetMemory()
    {
        for (int i = 0; i < SceneController.Instance.LevelsInOrderAscending.Length; i++)
        {
            //set all to 0
            PlayerPrefs.SetInt(SceneController.Instance.LevelsInOrderAscending[i].ToString(), 0);
            //set first to 1
            PlayerPrefs.SetInt(SceneController.Instance.LevelsInOrderAscending[0].ToString(), 1);
        }
    }

    public void UnlockNextLevel()
    {
        //if isnt last level
        if((SceneController.Instance.currentLevelIndex + 1) > SceneController.Instance.LevelsInOrderAscending.Length)
        {
            Debug.Log("unlock: " + SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex + 1].ToString());
            PlayerPrefs.SetInt(SceneController.Instance.LevelsInOrderAscending[SceneController.Instance.currentLevelIndex + 1].ToString(), 1);
        }
    }

    public bool GetUnlockedStateByIndex(int index)
    {
        if(PlayerPrefs.GetInt(SceneController.Instance.LevelsInOrderAscending[index].ToString()) == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
