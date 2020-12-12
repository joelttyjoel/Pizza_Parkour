using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EasyUIAnimator;
using UnityEngine.EventSystems;

public class ButtonStuff : MonoBehaviour
{

    [Header("Select what do")]
    public bool animateOnSelect;
    public bool onClickSelectOther;
    public bool selectOnMouseover;
    public bool selectOnEnable;
    public bool hasAnimation;
    public bool isOnLevelselect;

    [Header("Fill if needed")]
    public GameObject objectToSelect;

    private Image thisImage;
    private Button thisButton;
    private UIFixedAnimation thisAnimation;

    private bool animationIsPlaying = false;

    private void Start()
    {
        thisImage = GetComponent<Image>();
        thisButton = GetComponent<Button>();
        thisButton.onClick.AddListener(OnClick);
        if(selectOnEnable) EventSystem.current.SetSelectedGameObject(this.gameObject);
        if (!hasAnimation) return;

        thisAnimation = GetComponent<UIFixedAnimation>();
        animationIsPlaying = thisAnimation.playOnEnable;
    }

    private void Update()
    {
        if (!hasAnimation) return;
        //if (thisButton.interactable == false) return;
        //if(EventSystem.current.currentSelectedGameObject != null) Debug.Log(EventSystem.current.currentSelectedGameObject.name);
        //no deselect event in selecthandler
        if (EventSystem.current.currentSelectedGameObject == this.gameObject && !animationIsPlaying)
        {
            thisAnimation.Play();
            animationIsPlaying = true;
        }
        if(EventSystem.current.currentSelectedGameObject != this.gameObject && animationIsPlaying)
        {
            thisAnimation.Pause();
            if(thisAnimation.animType == AnimationType.SCALE)
            {
                transform.localScale = new Vector3(thisAnimation.startV3.x, thisAnimation.startV3.y, thisAnimation.startV3.z);
            }
            else if (thisAnimation.animType == AnimationType.MOVE)
            {
                transform.localPosition = new Vector3(thisAnimation.startV3.x, thisAnimation.startV3.y, thisAnimation.startV3.z);
            }
            animationIsPlaying = false;
        }
    }

    private void OnClick()
    {
        if (onClickSelectOther)
        {
            EventSystem.current.SetSelectedGameObject(objectToSelect);
        }
    }

    private void OnMouseEnter()
    {
        if(selectOnMouseover)
        {
            if(isOnLevelselect)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }
            else if(!SpanningUIController.Instance.levelSelectIsShowing)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
            }
        }
    }
}
