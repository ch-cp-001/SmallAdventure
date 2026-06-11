using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class ABMgr : SingletonBase<ABMgr>
{
    private ABMgr() { }

    private Dictionary<string,AssetBundle> abDic = new Dictionary<string, AssetBundle>();

    // ab包加载路径
    public string abPath
    {
        get
        {
            return Application.streamingAssetsPath+"/";
        }
    }

    public string mainPkgName
    {
        get
        {
            #if UNITY_IOS
                return "IOS";
            #elif UNITY_ANDRIOD
                return "Andriod";
            #else
                return "PC";
            #endif
        }

    }
    // 存放加载的主包，用于判断是否有加载主包
    private AssetBundle mainPkg = null;
    private AssetBundleManifest abMainfest = null;

    public void LoadMainABundle()
    {
        // 主包为空时需要先加载主包
        if (mainPkg == null)
        {
            string path = abPath + mainPkgName;
            mainPkg = AssetBundle.LoadFromFile(path);
            // 通过主包加载依赖包
            abMainfest = mainPkg.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }
    // 同步加载
    //public void LoadDependency(string pkgName)
    //{
    //    // 返回目标包路径
    //    string[] strs = abMainfest.GetAllDependencies(pkgName);
        
    //    for(int i = 0; i < strs.Length; i++)
    //    {
    //        if (!abDic.ContainsKey(strs[i]))
    //        {
    //            AssetBundle pkg = AssetBundle.LoadFromFile(strs[i]);
    //            if (pkg != null)
    //            {
    //                abDic.Add(strs[i],pkg);
    //            }
    //        }
    //    }

    //}

    ////同步加载 泛型
    //public T LoadABRes<T>(string pkgName, string resName) where T : Object
    //{
    //    // 当前包已加载，则依赖包不用重复加载
    //    if (abDic.ContainsKey(pkgName))
    //    {
    //        return abDic[pkgName].LoadAsset<T>(resName);
    //    }
    //    //从目标包加载资源        
    //    AssetBundle aBundle = AssetBundle.LoadFromFile(pkgName);
    //    // 加载为空说明目标包不存在，返回空值
    //    if (aBundle == null) return null;

    //    abDic.Add(pkgName,aBundle);
    //    //先加载依赖包
    //    LoadDependency(pkgName);

    //    // 从目标包中加载资源
    //    T t = aBundle.LoadAsset<T>(resName);
    //    if (t != null)
    //        if (t is GameObject)
    //            return GameObject.Instantiate(t);
    //        else
    //            return t;
    //    return null;
        
    //}

    //// 同步加载 非泛型 提供给lua使用  
    //public Object LoadABRes(System.Type type,string pkgName, string resName)
    //{
    //    // 当前包已加载，则依赖包不用重复加载
    //    if (abDic.ContainsKey(pkgName))
    //    {
    //        return abDic[pkgName].LoadAsset(resName,type);
    //    }
    //    //从目标包加载资源        
    //    AssetBundle aBundle = AssetBundle.LoadFromFile(pkgName);
    //    // 加载为空说明目标包不存在，返回空值
    //    if (aBundle == null) return null;

    //    abDic.Add(pkgName, aBundle);
    //    //先加载依赖包
    //    LoadDependency(pkgName);

    //    // 从目标包中加载资源
    //    Object t = aBundle.LoadAsset(resName,type);
    //    if (t != null)
    //        if (t is GameObject)
    //            return GameObject.Instantiate(t);
    //        else
    //            return t;
    //    return null;
    //}

    // 异步加载方法

    public void LoadABResAsync<T>(string pkgName, string resName, UnityAction<T> callback,bool isSync = false) where T : Object
    {
        MonoMgr.Instance.StartCoroutine(RealLoadABResAsync<T>(pkgName,resName,callback,isSync));
    }

    private IEnumerator RealLoadABResAsync<T>(string pkgName, string resName,UnityAction<T> callback, bool isSync = false) where T : Object
    {
        // 加载主包
        LoadMainABundle();

        string path = "";
        string[] strs = abMainfest.GetAllDependencies(pkgName);

        AssetBundleCreateRequest req;
        for (int i = 0; i < strs.Length; i++)
        {
            path = abPath + strs[i];
            if (!abDic.ContainsKey(strs[i]))
            {
                if (isSync)
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(path);
                    abDic.Add(strs[i], ab);
                }
                else
                {
                    // 先用null占位表示资源正在加载但还没有加载完成
                    abDic.Add(strs[i], null);
                    req = AssetBundle.LoadFromFileAsync(path);
                    yield return req;
                    abDic[strs[i]] = req.assetBundle;
                }
            }
            else
            {
                // 字典中存在键但值为null说明资源正在异步加载中，等待资源加载完毕
                while (abDic[strs[i]] == null)
                    yield return 0;
            }
        }
        path = abPath + pkgName;
        if (!abDic.ContainsKey(pkgName))
        {
            if (isSync)
            {
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                abDic.Add(pkgName, ab);
            }
            else
            {
                abDic.Add(pkgName, null);
                //从目标包加载资源        
                req = AssetBundle.LoadFromFileAsync(path);
                yield return req;
                abDic[pkgName] = req.assetBundle;
            }
        }
        else
        {
            // 字典中存在键但值为null说明资源正在异步加载中，休息一帧等待资源加载完毕
            while (abDic[pkgName] == null)
                yield return 0;
        }

        if (isSync)
        {
            T t = abDic[pkgName].LoadAsset<T>(resName);
            callback?.Invoke(t);
        }
        else
        {
            // 从目标包中加载资源
            AssetBundleRequest abReq = abDic[pkgName].LoadAssetAsync<T>(resName);
            yield return abReq;
            callback?.Invoke(abReq.asset as T);
        }
    }

    // 异步加载方法  非泛型
    public void LoadABResAsync(string pkgName, string resName,System.Type type, UnityAction<Object> callback,bool isSync = false)
    {
        MonoMgr.Instance.StartCoroutine(RealLoadABResAsync(pkgName, resName,type, callback,isSync));
    }

    private IEnumerator RealLoadABResAsync(string pkgName, string resName, System.Type type, UnityAction<Object> callback, bool isSync = false)
    {
        // 加载主包
        LoadMainABundle();
        string path = "";
        string[] strs = abMainfest.GetAllDependencies(pkgName);

        AssetBundleCreateRequest req;
        for (int i = 0; i < strs.Length; i++)
        {
            path = abPath + strs[i];
            if (!abDic.ContainsKey(strs[i]))
            {
                if(isSync)
                {
                    AssetBundle ab = AssetBundle.LoadFromFile(path);
                    abDic.Add(strs[i], ab);
                }
                else
                {
                    // 先用null占位表示资源正在加载但还没有加载完成
                    abDic.Add(strs[i], null);
                    req = AssetBundle.LoadFromFileAsync(path);
                    yield return req;
                    abDic[strs[i]] = req.assetBundle;
                }
                
            }
            else
            {
                // 字典中存在键但值为null说明资源正在异步加载中，等待资源加载完毕
                while (abDic[strs[i]] == null)
                    yield return 0;
            }
        }
        path = abPath + pkgName;
        if (!abDic.ContainsKey(pkgName))
        {
            if (isSync)
            {
                AssetBundle ab = AssetBundle.LoadFromFile(path);
                abDic.Add(pkgName, ab);
            }
            else
            {
                abDic.Add(pkgName, null);
                //从目标包加载资源        
                req = AssetBundle.LoadFromFileAsync(path);
                yield return req;
                abDic[pkgName] = req.assetBundle;
            }
        }
        else
        {
            // 字典中存在键但值为null说明资源正在异步加载中，休息一帧等待资源加载完毕
            while (abDic[pkgName] == null)
                yield return 0;
        }

        if (isSync)
        {
            Object t = abDic[pkgName].LoadAsset(resName,type);
            callback?.Invoke(t);
        }
        else
        {
            // 从目标包中加载资源
            AssetBundleRequest abReq = abDic[pkgName].LoadAssetAsync<Object>(resName);
            yield return abReq;
            callback?.Invoke(abReq.asset);
        }
    }

    public void UnLoad(string pkgName,UnityAction<bool> callback)
    {
        if (abDic.ContainsKey(pkgName))
        {
            if(abDic[pkgName] != null)
            {
                abDic[pkgName].Unload(false);
                abDic.Remove(pkgName);
                // 表示卸载成功
                callback?.Invoke(true);
            }
            else
            {
                //正在异步加载，不允许卸载
                callback?.Invoke(false);
            }
        }
    }

    public void UnLoadAllAssetBundled()
    {
        // 清理之前停止所有协程
        MonoMgr.Instance.StopAllCoroutines();
        AssetBundle.UnloadAllAssetBundles(false);
        abDic.Clear();

        mainPkg = null;
        abMainfest = null;
    }
}
