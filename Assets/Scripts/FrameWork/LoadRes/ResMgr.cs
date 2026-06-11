using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResInfoBase{
    // 引用计数
    public int refNum;

    public void PlusRef()
    {
        refNum++;
    }

    public void MinusRef()
    {
        refNum--;
        if (refNum < 0)
            Debug.LogError("引用计数小于0，请检查资源的加载和卸载是否配对");
    }
}

public class ResInfo<T> : ResInfoBase
{
    // 资源
    public T asset;
    // 回调函数委托
    public UnityAction<T> action;
    // 使用的协程，用于之后关闭
    public Coroutine coroutine;
    // 标识 是否需要真正卸载  某些资源即使引用计数归零了，也希望暂时保留
    public bool needDel;

}

//Resources加载资源
public class ResMgr : SingletonBase<ResMgr>
{
    private ResMgr() { }
    private Dictionary<string, ResInfoBase> resDic = new Dictionary<string, ResInfoBase>();

    /// <summary>
    /// Resources同步加载资源方法
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    public T Load<T>(string path) where T : Object
    {
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> res;
        if (!resDic.ContainsKey(resName))
        {
            T t = Resources.Load<T>(path);
            res = new ResInfo<T>();
            res.asset = t;
            resDic.Add(resName, res);
            res.PlusRef();
            return t;
        }
        else
        {
            res = resDic[resName] as ResInfo<T>;
            res.PlusRef();
            if (res.asset == null)
            {
                // 异步加载还在加载中。停止异步加载，直接回调返回
                T t = Resources.Load<T>(path);
                res.asset = t;
                res.action?.Invoke(t);
                MonoMgr.Instance.StopCoroutine(res.coroutine);
                res.action = null;
                res.coroutine = null;
                return t;
            }
            else
            {
                return res.asset;
            }
        }    
        
    }

    /// <summary>
    /// Resources异步加载资源的方法
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">Resources下的资源路径</param>
    /// <param name="callback">回调函数，给调用者返回加载的资源</param>
    public void LoadAsync<T>(string path,UnityAction<T> callback) where T : Object
    {
        string resName = path+"_"+typeof(T).Name;
        ResInfo<T> res;
        if (!resDic.ContainsKey(resName))
        {
            res = new ResInfo<T>();
            res.PlusRef();
            res.action = callback;
            resDic.Add(resName, res);
            res.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync<T>(path));
        }
        else
        {
            res = resDic[resName] as ResInfo<T>;
            res.PlusRef();
            if (res.asset == null)
            {
                res.action += callback;
            }
            else
            {
                callback?.Invoke(res.asset);
            }
        }
        
    }

    private IEnumerator ReallyLoadAsync<T>(string path) where T : Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        yield return request;
        string resName = path + "_" + typeof(T).Name;
        ResInfo<T> res = resDic[resName] as ResInfo<T>;
        res.asset = request.asset as T;

        if (res.refNum == 0)
        {
            UnLoad<T>(path,null,res.needDel,false);
        }
        else
        {
            res.action?.Invoke(request.asset as T);
            res.coroutine = null;
            res.action = null;
        }
    }

    // 非泛型方法
    [System.Obsolete("请使用泛型方法,该方法不能与泛型方法混用，资源类型不同")]
    public void LoadAsync(string path, System.Type type, UnityAction<Object> callback) 
    {
        string resName = path + "_" + type.Name;
        ResInfo<Object> res;
        if (!resDic.ContainsKey(resName))
        {
            res = new ResInfo<Object>();
            res.PlusRef();
            res.action = callback;
            resDic.Add(resName, res);
            res.coroutine = MonoMgr.Instance.StartCoroutine(ReallyLoadAsync(path,type));
        }
        else
        {
            res = resDic[resName] as ResInfo<Object>;
            res.PlusRef();
            if (res.asset == null)
            {
                res.action += callback;
            }
            else
            {
                callback?.Invoke(res.asset);
            }
        }
    }

    private IEnumerator ReallyLoadAsync(string path, System.Type type) 
    {
        ResourceRequest request = Resources.LoadAsync<Object>(path);
        yield return request;
        string resName = path + "_" + type.Name;
        ResInfo<Object> res = resDic[resName] as ResInfo<Object>;
        res.asset = request.asset;

        if (res.refNum == 0)
        {
            UnLoad(path,type,null,res.needDel,false);
        }
        else
        {
            res.action?.Invoke(request.asset);
            res.coroutine = null;
            res.action = null;
        }
    }

    // 卸载单个资源
    // isDel 某个资源是否需要移除
    public void UnLoad<T>(string path,UnityAction<T> callback=null, bool isDel=true,bool isSub = true)
    {
        string resName = path + "_" + typeof(T).Name;

        if (resDic.ContainsKey(resName))
        {
            ResInfo<T> res = resDic[resName] as ResInfo<T>;
            if (isSub)
                res.MinusRef();
            res.needDel = isDel;
            // 说明还在异步加载中
            if (res.asset == null)
            {
                // 正在加载中，提供一个标识，加载完成后卸载
                //res.needDel = true;
                // 移除单个回调，可能有其他对象还要使用这个资源，不能直接删除
                res.action -= callback;
            }
            else if(res.refNum == 0 && isDel)
            {
                Resources.UnloadAsset(res.asset as Object);
                resDic.Remove(resName);
            }
        }
        
    }

    public void UnLoad(string path,System.Type type, UnityAction<Object> callback=null, bool isDel=true,bool isSub = true)
    {
        string resName = path + "_" + type.Name;
        if (resDic.ContainsKey(resName))
        {
            ResInfo<Object> res = resDic[resName] as ResInfo<Object>;
            if (isSub) 
                res.MinusRef();
            res.needDel = isDel;
            if (res.asset == null)
            {
                // 正在加载中，提供一个标识，加载完成后卸载
                //res.needDel = true;
                res.action -= callback;
            }
            else if(res.refNum == 0 && isDel)
            {
                Resources.UnloadAsset(res.asset);
                resDic.Remove(resName);
            }
        }
    }

    // 卸载所有资源
    public void UnloadUnusedAssets(UnityAction callback)
    {
        MonoMgr.Instance.StartCoroutine(ReallyUnloadUnusedAssets(callback));
        resDic.Clear();
    }

    private IEnumerator ReallyUnloadUnusedAssets(UnityAction callback)
    {
        List<string> lists = new List<string>();
        foreach (var key in resDic.Keys)
        {
            if (resDic[key].refNum == 0)
            {
                lists.Add(key);
            }
        }
        foreach (string str in lists)
        {
            resDic.Remove(str);
        }

        AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
        yield return asyncOperation;
        callback?.Invoke();
    }

    // 清空字典，如果不想使用引用计数功能就需要在过场景时调用回收资源
    public void Clear(UnityAction callback)
    {
        resDic.Clear();
        MonoMgr.Instance.StartCoroutine(ReallyClear(callback));
    }

    private IEnumerator ReallyClear(UnityAction callback)
    {
        AsyncOperation asyncOperation = Resources.UnloadUnusedAssets();
        yield return asyncOperation;
        callback?.Invoke();
    }

    public int GetRefNum<T>(string path)
    {
        string resName = path + "_" + typeof(T).Name;
        if (resDic.ContainsKey(resName))
            return resDic[resName].refNum;
        return -10;
    }
}
