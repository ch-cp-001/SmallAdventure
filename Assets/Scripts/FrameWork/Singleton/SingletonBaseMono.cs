using UnityEngine;


/// <summary>
/// 继承自MonoBehaviour的单例基类，使用时只需要继承这个类并指定类型参数即可
/// </summary>
/// <typeparam name="T"></typeparam>
public class SingletonBaseMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject();
                // 将单例对象命名为类名，方便在Hierarchy中查找
                obj.name = typeof(T).Name;
                instance = obj.AddComponent<T>();
                // 使单例对象在场景切换时不被销毁
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }
}
