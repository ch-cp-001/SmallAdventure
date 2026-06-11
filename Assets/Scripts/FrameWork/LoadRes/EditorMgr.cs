#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EditorMgr : SingletonBase<EditorMgr>
{
    private EditorMgr() { }

    private string rootPath = "Assets/Editor/ArtRes/";

    // 加载单个资源
    public T LoadEditorRes<T>(string path) where T : Object
    {
        // 只有编辑器开发时才会使用
        string suffixName = "";
        if (typeof(T) == typeof(GameObject))
            suffixName = ".prefab";
        else if (typeof(T) == typeof(Sprite))
            suffixName = ".png";
        else if (typeof(T) == typeof(Material))
            suffixName = ".mat";
        else if (typeof(T) == typeof(AudioClip))
            suffixName = ".mp3";
        T res = AssetDatabase.LoadAssetAtPath<T>(rootPath+path+suffixName);
        Debug.Log(res);
        return res;

    }

    //加载图集中的一个图片资源
    public Sprite LoadSprite(string path,string spriteName)
    {
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);
        foreach (var item in sprites)
        {
            if (item.name == spriteName)
                return item as Sprite;
        }
        return null;
    }

    public Dictionary<string,Sprite> LoadAtlas(string path)
    {
        Dictionary<string,Sprite> dic = new Dictionary<string,Sprite>();
        Object[] sprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);
        foreach (var item in sprites)
        {
            dic.Add(item.name, item as Sprite);
        }
        return dic;
    }
}

#endif
