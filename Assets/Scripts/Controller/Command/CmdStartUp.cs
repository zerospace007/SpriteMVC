#region 引用空间
using Framework;
using Framework.Core;
using Framework.Interfaces;
using Framework.Utility;
using UnityEngine;
#endregion

public class CmdStartUp : SimpleCommand
{

    public override void Execute(INotification message)
    {
        if (!Util.CheckEnvironment()) return;

        if (GameConst.LogMode)
        {
            GameObject debugObj = new GameObject("DebugModeObj");
            debugObj.AddComponent<DebugModeView>();
            Object.DontDestroyOnLoad(debugObj);
        }

        InitManager();
    }

    /// <summary>
    /// 初始化添加管理器
    /// </summary>
    private void InitManager()
    {
        Singleton.GetInstance<PoolManager>();
        Singleton.GetInstance<TipsManager>();
        Singleton.GetInstance<UIManager>().ShowUI2(DownloadView.Name);//打开解包下载界面
        Singleton.GetInstance<ResourceManager>();
        Singleton.GetInstance<DownloadManager>();
        Singleton.GetInstance<LuaManager>();
        //Singleton.GetInstance<TimerManager>();
        Singleton.GetInstance<NetworkManager>();
        Singleton.GetInstance<GameManager>();
    }
}