using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnMouseHover : MonoBehaviour
{
    public Sprite idleImage;
    public Sprite activeImage;

    private Image thisImage;

    void Start()
    {
        thisImage = GetComponent<Image>();

        thisImage.sprite = idleImage;
    }

    private void OnMouseEnter()
    {
        if (SpanningUIController.Instance.levelSelectIsShowing) return;

        thisImage.sprite = activeImage;
    }
    private void OnMouseExit()
    {
        thisImage.sprite = idleImage;
    }
}
