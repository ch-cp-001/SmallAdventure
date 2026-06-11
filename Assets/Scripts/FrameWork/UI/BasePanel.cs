using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class BasePanel : MonoBehaviour
{
    
    protected Dictionary<string,UIBehaviour> uiDic = new Dictionary<string,UIBehaviour>();

    private List<string> defaultName = new List<string>() { "Image", "Text (TMP)", "RawImage", "Toggle",
                                                            "Slider","Scrollbar","Scroll View","Button",
                                                            "Dropdown","InputField (TMP)","Text (Legacy)",
                                                            "Button (Legacy)","Dropdown (Legacy)","InputField (Legacy)","Background",
                                                            "Checkmark","Label","Fill","Handle","Viewport",
                                                            "Scrollbar Horizontal","Scrollbar Vertical","Placeholder","Text"};

    void Awake()
    {
        LoadControls<Button>();
        LoadControls<Toggle>();
        LoadControls<Slider>();
        LoadControls<ScrollRect>();
        LoadControls<Dropdown>();
        LoadControls<InputField>();
        LoadControls<Text>();
        LoadControls<TextMeshProUGUI>();
        LoadControls<Image>();
        LoadControls<RawImage>();
    }

    // 获取面板中指定类型的组件
    protected void LoadControls<T>() where T:UIBehaviour
    {
        T[] ts = GetComponentsInChildren<T>(true);
        for(int i = 0; i < ts.Length; i++)
        {
            string controlName = ts[i].name;
            if (!defaultName.Contains(controlName) && !uiDic.ContainsKey(controlName))
            {
                uiDic.Add(controlName, ts[i]);
                if(typeof(T) == typeof(Button))
                {
                    (uiDic[controlName] as Button).onClick.AddListener(() =>
                    {
                        BtnClick(controlName);
                    });
                }
                else if(typeof(T) == typeof(Slider))
                {
                    (uiDic[controlName] as Slider).onValueChanged.AddListener((value) =>
                    {
                        SliderValueChanged(controlName,value);
                    });
                }
                else if(typeof(T) == typeof(Toggle))
                {
                    (uiDic[controlName] as Toggle).onValueChanged.AddListener((value) =>
                    {
                        ToggleValueChanged(controlName, value);
                    });
                }
            }
        }
    }

    protected virtual void BtnClick(string name)
    {

    }

    protected virtual void SliderValueChanged(string name,float value)
    {

    }

    protected virtual void ToggleValueChanged(string name, bool value)
    {

    }

    public abstract void ShowMe();
    public abstract void HideMe();

    public T GetControl<T>(string name) where T:UIBehaviour
    {
        if (uiDic.ContainsKey(name))
        {
            if (uiDic[name] != null)
            {
                return uiDic[name] as T;
            }
            else
            {
                Debug.LogError($"不存在所需类型为{typeof(T).Name}名字为{name}的组件");
                return null;
            }

        }
        Debug.LogError($"不存在所需类型为{typeof(T).Name}名字为{name}的组件");
        return null;
    }


}
