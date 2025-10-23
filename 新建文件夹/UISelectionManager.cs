using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;

public class UISelectionManager : MonoBehaviour
{
    public static UISelectionManager Instance;

    [Header("Selection Frame")]
    public RectTransform selectionFrame;
    public Vector2 offset = Vector2.zero;

    private RectTransform lastTarget;

    private bool initialized = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        //DetectInputSource();

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current != null && current.TryGetComponent(out Button btn))
        {
            RectTransform target = btn.GetComponent<RectTransform>();
            if (target != null && target != lastTarget)
            {
                if (initialized)
                    FollowTo(target);
            }

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (btn.interactable)
                {
                    btn.onClick.Invoke();
                }
            }
        }
        else
        {
            Hide();
        }
    }

    // 检测输入类型
    //void DetectInputSource()
    //{
    //    if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
    //        Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
    //        Input.GetKeyDown(KeyCode.Tab) ||
    //        Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || 
    //        Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f ||
    //        Input.GetKeyDown(KeyCode.Joystick1Button6) ||
    //        Input.GetKeyDown(KeyCode.Joystick1Button7) ||
    //        Input.GetKeyDown(KeyCode.JoystickButton0))
    //    {
           
    //    }
    //}

    // 显示选中框
    public void FollowTo(RectTransform target)
    {
        if (target == null) return;

        selectionFrame.gameObject.SetActive(true);
        lastTarget = target;

        Vector2 worldPos = target.position;
        Vector2 localPos;
        RectTransform parentRect = selectionFrame.parent as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, worldPos, null, out localPos))
        {
            selectionFrame.localPosition = localPos + offset;
        }
    }

    // 隐藏选中框
    public void Hide()
    {
        selectionFrame.gameObject.SetActive(false);
        lastTarget = null;
    }

    // 设置初始选中按钮（在面板启用时调用）
    public void SetInitialSelected(GameObject button)
    {
        if (button == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(button);

        if (button.TryGetComponent(out RectTransform rt))
        {
            FollowTo(rt);
        }
    }

    // 注册按钮悬停事件（可用于初始化所有按钮）
    public void RegisterButton(Button btn)
    {
        if (btn == null) return;

        EventTrigger trigger = btn.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();

        trigger.triggers ??= new List<EventTrigger.Entry>();

        EventTrigger.Entry entry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        entry.callback.AddListener((data) =>
        {
            FollowTo(btn.GetComponent<RectTransform>());
            EventSystem.current.SetSelectedGameObject(btn.gameObject);
        });

        trigger.triggers.Add(entry);
    }

    public void SetSelectionParent(RectTransform parent)
    {
        if (selectionFrame != null && parent != null)
        {
            selectionFrame.SetParent(parent, worldPositionStays: false);
        }
    }

    public void SetInitialSelectedDelayed(GameObject button)
    {
        StartCoroutine(DelaySet(button));
    }

    private IEnumerator DelaySet(GameObject button)
    {
        yield return null;
        SetInitialSelected(button);
        initialized = true;
    }
}