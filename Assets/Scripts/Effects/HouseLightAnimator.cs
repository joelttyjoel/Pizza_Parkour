using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseLightAnimator : MonoBehaviour
{
    public bool animate;
    public SpriteRenderer[] lights;
    public float timePerImage;

    private int currentIndex = 0;
    private float currentTime = 0;

    private void Start()
    {
        foreach(SpriteRenderer a in lights)
        {
            a.enabled = false;
        }

        animate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!animate) return;

        currentTime += Time.deltaTime;

        if (currentTime > timePerImage)
        {
            lights[currentIndex].enabled = false;
            currentIndex++;
            if (currentIndex == lights.Length) currentIndex = 0;
            lights[currentIndex].enabled = true;
            currentTime = 0f;
        }
    }
}
