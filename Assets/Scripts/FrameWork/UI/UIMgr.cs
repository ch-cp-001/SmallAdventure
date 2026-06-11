using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum E_UILayer
{
    Bottom,
    Middle,
    Top,
    System
}

public class PanelInfoBase
{

}

public class PanelInfo<T> : PanelInfoBase where T : BasePanel
{
    public BasePanel panel;
    public UnityAction<T> action;
    public bool needHide= false;

    public PanelInfo(BasePanel panel,UnityAction<T> action)
    {
        this.panel = panel;
        this.action += action;
    }
}


public class UIMgr : SingletonBase<UIMgr>
{
    private Canvas canvas;
    private GameObject eventSystem;
    private Camera uiCamera;

    private Transform bottom;
    private Transform middle;
    private Transform top;
    private Transform system;

    private Dictionary<string, PanelInfoBase> panelDic = new Dictionary<string, PanelInfoBase>();

    private UIMgr()
    {
        uiCamera = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/UICamera")).GetComponent<Camera>();

        canvas = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/Canvas")).GetComponent<Canvas>();
        canvas.worldCamera = uiCamera;

        eventSystem = GameObject.Instantiate(ResMgr.Instance.Load<GameObject>("UI/EventSystem"));

        GameObject.DontDestroyOnLoad(uiCamera.gameObject);
        GameObject.DontDestroyOnLoad(canvas.gameObject);
        GameObject.DontDestroyOnLoad(eventSystem);

        // 获取Canvas中的层级
        bottom = canvas.transform.Find("Bottom");
        middle = canvas.transform.Find("Middle");
        top = canvas.transform.Find("Top");
        system = canvas.transform.Find("System");
    }

    public Transform GetUILayer(E_UILayer layer)
    {
        switch (layer)
        {
            case E_UILayer.Bottom:
                return bottom;
            case E_UILayer.Middle:
                return middle;
            case E_UILayer.Top:
                return top;
            case E_UILayer.System:
                return system;
            default:
                return null;
        }
    }

    /// <summary>
    /// 显示面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <param name="layer">面板所在层级</param>
    /// <param name="callback">回调函数 返回显示的面板</param>
    /// <param name="isSync">是否采用同步加载</param>
    public void ShowPanel<T>(UnityAction<T> callback,bool isSync = true, E_UILayer layer = E_UILayer.Middle) where T:BasePanel
    {
        string panelName = typeof(T).Name;
        PanelInfo<T> panelInfo = null;
        if (panelDic.ContainsKey(panelName))
        {
            panelInfo = (panelDic[panelName] as PanelInfo<T>);
            if (panelInfo.panel != null)
            {
                // 如果是失活状态，需要重新激活
                if (!panelInfo.panel.gameObject.activeSelf)
                    panelInfo.panel.gameObject.SetActive(true);
                panelInfo.panel.ShowMe();
                callback?.Invoke(panelInfo.panel as T);
            }
            else
            {
                // 正在异步加载中，说明这个面板肯定是要显示的
                panelInfo.needHide = false;
                // 正在异步加载，等待
                if(panelInfo.action != null)
                    panelInfo.action += callback;
            }
        }
        else
        {
            panelDic.Add(panelName, new PanelInfo<T>(null,callback));
            ABResMgr.Instance.LoadABResAsync<GameObject>("ui", typeof(T).Name, (objRes) =>
            {
                panelInfo = (panelDic[panelName] as PanelInfo<T>);
                if (panelInfo.needHide)
                {
                    // 记得移除字典中的数据
                    panelDic.Remove(panelName);
                    return;
                }
                // 获取要设置的层级，传错或不传默认为middle
                Transform father = GetUILayer(layer);
                if (father == null)
                {
                    father = middle;
                }

                GameObject obj = GameObject.Instantiate(objRes);
                obj.name = panelName;
                obj.transform.SetParent(father, false);
                T panel = obj.GetComponent<T>();
                if (panel != null)
                {
                    panelInfo.panel = panel;
                    //panelDic.Add(panelName, panel);
                    panel.ShowMe();
                    panelInfo.action?.Invoke(panel);
                }
                else
                {
                    panel = obj.AddComponent<T>();
                    if (panel != null)
                    {
                        panelInfo.panel = panel;
                        panel.ShowMe();
                        panelInfo.action?.Invoke(panel);
                    }
                    else
                    {
                        Debug.LogError($"面板中没有找到对应类型的面板脚本，请检查面板名字{panelName}是否正确");
                        panelInfo.action?.Invoke(null);
                    }
                }
                // 回调完成后让callback置空
                panelInfo.action = null;
            }, isSync);
        }        
    }

    /// <summary>
    /// 隐藏面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    public void HidePanel<T>(bool isDes=false) where T:BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = (panelDic[panelName] as PanelInfo<T>);
            if (panelInfo.panel != null)
            { 
                panelInfo.panel.HideMe();
                if (isDes)
                {
                    GameObject.Destroy(panelInfo.panel.gameObject);
                    panelDic.Remove(panelName);
                }
                else
                {
                    panelInfo.panel.gameObject.SetActive(false);
                }
            }
            else
            {
                panelInfo.needHide = true;
                panelInfo.action = null;
            }
        }
    }
    /// <summary>
    /// 获取面板
    /// </summary>
    /// <typeparam name="T">面板类型</typeparam>
    /// <returns></returns>
    public void GetPanel<T>(UnityAction<T> callback)  where T:BasePanel
    {
        string panelName = typeof(T).Name;
        if (panelDic.ContainsKey(panelName))
        {
            PanelInfo<T> panelInfo = (panelDic[panelName] as PanelInfo<T>);
            if(panelInfo.panel != null && !panelInfo.needHide)
            {
                callback?.Invoke(panelInfo.panel as T);
            }
            else if(callback != null)
            {
                panelInfo.action += callback;
            }
        }
    }

    /// <summary>
    /// 添加自定义事件监听
    /// </summary>
    /// <param name="control">指定控件</param>
    /// <param name="type">事件类型</param>
    /// <param name="callback">回调函数</param>
    public void AddCustomEventListener(UIBehaviour control, EventTriggerType type, UnityAction<BaseEventData> callback)
    {
        EventTrigger et = control.gameObject.GetComponent<EventTrigger>();
        if (et == null)
        {
            et = control.gameObject.AddComponent<EventTrigger>();
        }
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(callback);
        et.triggers.Add(entry);


    }
}
