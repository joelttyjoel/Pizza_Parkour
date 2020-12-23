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
    public bool isAboveMainMenu;

    [Header("Fill if needed")]
    public GameObject objectToSelect;

    private Image thisImage;
    private Button thisButton;
    private UIFixedAnimation thisAnimation;

    private bool animationIsPlaying = false;
    private bool hasEntered = false;

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

    //all this is dirty but i hate ui dw its fine
    private void Update()
    {
        if(IsMouseOverThisObject() && !hasEntered)
        {
            Debug.Log("IS ON" + gameObject.name);
            MouseEnter();
            hasEntered = true;
        }
        if(!IsMouseOverThisObject() && hasEntered)
        {
            hasEntered = false;
        }

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

    private bool IsMouseOverThisObject()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;

        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        bool isOnThisObject = false;
        for (int i = 0; i < raycastResultList.Count; i++)
        {
            if (raycastResultList[i].gameObject == this.gameObject)
            {
                isOnThisObject = true;
            }
        }

        return isOnThisObject;
    }

    private void OnClick()
    {
        //Debug.Log("clicked" + gameObject.name);
        if (onClickSelectOther)
        {
            EventSystem.current.SetSelectedGameObject(objectToSelect);
        }
    }

    private void MouseEnter()
    {
        if (selectOnMouseover)
        {
            if (SpanningUIController.Instance.winScreenIsShowing)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
                SpanningUIController.Instance.PlaySelectSound();
            }
            else if (isAboveMainMenu)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
                SpanningUIController.Instance.PlaySelectSound();
            }
            else if(!SpanningUIController.Instance.levelSelectIsShowing && !SpanningUIController.Instance.settingsIsShowing)
            {
                EventSystem.current.SetSelectedGameObject(this.gameObject);
                SpanningUIController.Instance.PlaySelectSound();
            }
        }
    }
}
