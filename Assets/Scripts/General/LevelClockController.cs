using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelClockController : MonoBehaviour
{
    public Text clockText;
    public float currentTimeClock = 0f;
    public bool runClock = true;

    //singleton
    public static LevelClockController Instance;
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

    private void Update()
    {
        if (!runClock) return;

        currentTimeClock += Time.deltaTime;
        clockText.text = currentTimeClock.ToString("F2");
    }
}
