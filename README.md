# YeungclueFramework_EventManager
简单实现一下常用的事件管理模块，管理全局事件的订阅和触发，支持0-4个参数，支持弱事件
# 怎么使用？
注册与触发弱事件
```C#
    //注册事件
    private void Awake()
    {
         EventManager.Instance.AddWeakListener("GameStart",GameStart, this);
    }

    //触发事件
    private void TriggerEvent()
    {
         EventCenter.Instance.AddWeakListener("GameStart",GamePause, this);
    }
```
普通事件
```C#
        void Start()
        {
            // 示例：添加事件监听
            EventManager.Instance.AddListener("TestEvent", OnTestEvent);
        }
        
        void OnDestroy()
        {
            // 示例：移除事件监听
            EventManager.Instance.RemoveListener("TestEvent", OnTestEvent);
        }
            
            // 示例：触发事件
        void TriggerEvent()
        {
            EventManager.Instance.TriggerEvent("TestEvent");
        }
```
