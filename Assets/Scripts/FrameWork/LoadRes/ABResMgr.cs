using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// 整合的AB包加载方法
public class ABResMgr : SingletonBase<ABResMgr>
{
    private ABResMgr() { }
    // 表示是否采用编辑器模式加载
    bool isDebug = true;

    public void LoadABResAsync<T>(string pkgName, string resName, UnityAction<T> callback, bool isSync = false) where T : Object
    {
#if UNITY_EDITOR
        if (isDebug)
        {
            // 规定editor中的文件存放在对应的ab包名字的文件夹中
            T t = EditorMgr.Instance.LoadEditorRes<T>($"{pkgName}/{resName}");
            callback?.Invoke(t);
        }
        else
        {
            ABMgr.Instance.LoadABResAsync<T>(pkgName,resName,callback,isSync);
        }

#else 
        ABMgr.Instance.LoadABResAsync<T>(pkgName,resName,callback,isSync);
#endif
    }
}
