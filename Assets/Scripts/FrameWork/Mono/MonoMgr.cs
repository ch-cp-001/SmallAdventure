using UnityEngine.Events;


/// <summary>
/// Mono模块用于为没有继承MonoBehaviour的脚本提供帧函数或定时函数以及协程
/// </summary>
public class MonoMgr : SingletonBaseMono<MonoMgr>
{
    // 协程不需要自己定义，该函数本身继承MonoBehaviour，可以直接调用StartCoroutine和StopCoroutine方法，
    // 但要注意只能通过函数方法调用，不能用字符串。
    public event UnityAction myUpdate;
    public event UnityAction myFixedUpdate;
    public event UnityAction myLateUpdate;

    //向外部提供添加和移除帧更新函数的方法
    public void AddUpdateListener(UnityAction action)
    {
        myUpdate += action;
    }
    public void RemoveUpdateListener(UnityAction action)
    {
        myUpdate -= action;
    }
    public void AddFixedUpdateListener(UnityAction action)
    {
        myUpdate += action;
    }
    public void RemoveFixedUpdateListener(UnityAction action)
    {
        myUpdate -= action;
    }
    public void AddLateUpdateListener(UnityAction action)
    {
        myUpdate += action;
    }
    public void RemoveLateUpdateListener(UnityAction action)
    {
        myUpdate -= action;
    }


    private void Update()
    {
        myUpdate?.Invoke();
    }

    private void FixedUpdate()
    {
        myFixedUpdate?.Invoke();
    }

    private void LateUpdate()
    {
        myLateUpdate?.Invoke();
    }

}
