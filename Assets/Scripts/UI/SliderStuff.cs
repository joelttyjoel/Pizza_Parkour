using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SliderStuff : MonoBehaviour
{
    public string playerPrefNameToUpdate;
    public AudioMixer mixerToSet;
    public Slider thisSlider;

    private float volume;

    private void Start()
    {
        thisSlider.onValueChanged.AddListener(delegate { OnValueChanged(); });
    }

    public void UpdateValues()
    {
        float temp = 0f;
        mixerToSet.GetFloat("Master", out temp);
        thisSlider.value = temp;
    }

    public void SetValues(float value)
    {
        mixerToSet.SetFloat("Master", value);
    }

    public void OnValueChanged()
    {
        volume = thisSlider.value;
        PlayerPrefs.SetFloat(playerPrefNameToUpdate, thisSlider.value);

        if (volume == thisSlider.minValue) volume = -100f;

        mixerToSet.SetFloat("Master", volume);
    }
}
