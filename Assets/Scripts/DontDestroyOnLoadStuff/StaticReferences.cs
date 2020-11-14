﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticReferences : MonoBehaviour
{
    public void SceneController_GoToMainMenu()
    {
        SceneController.Instance.GoToMainMenu();
    }

    public void SceneController_SwitchToLevelByIndex(int index)
    {
        SceneController.Instance.SwitchToLevelByIndex(index);
    }

    public void SceneController_ExitGame()
    {
        SceneController.Instance.ExitGame();
    }

    public void SpanningUIController_ToggleLevelSelect()
    {
        SpanningUIController.Instance.ToggleLevelSelect();
    }

    public void SpanningUIController_ShowWinScreen()
    {
        SpanningUIController.Instance.ShowWinScreen();
    }
}
