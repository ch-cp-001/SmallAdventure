using System;
using System.Reflection;
using UnityEngine;


/// <summary>
/// 不继承monobehaviour的单例基类，适用于不需要挂载在游戏对象上的单例类
/// </summary>
/// 1.解决单例唯一性问题：通过反射创建实例
public abstract class SingletonBase<T> where T: class//, new()   1.
{
    protected static readonly object lockObj = new object();

    private static T instance;
    public static T Instance
    {
        get
        {
            if(instance == null)
            {
                // 加锁解决线程安全问题
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        //instance = new T();   1.

                        // 通过反射创建实例，要求T必须有一个私有无参构造函数
                        Type type = typeof(T);
                        ConstructorInfo constructorInfo = type.GetConstructor(
                                            BindingFlags.Instance | BindingFlags.NonPublic, //表示成员私有方法
                                            null, //表示不使用绑定约束
                                            Type.EmptyTypes, //表示无参构造函数
                                            null); //表示不使用参数约束
                        if (constructorInfo == null)
                        {
                            Debug.LogError("请创建无参构造函数");
                        }
                        else instance = constructorInfo.Invoke(null) as T;
                    }
                }
            }

            
            return instance;
        }
    }
}
