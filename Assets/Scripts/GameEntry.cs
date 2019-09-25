using Framework.Core;
using UnityEngine;

/// <summary>
/// </summary>
public class GameEntry : MonoBehaviour
{
    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer)
            Screen.SetResolution(576, 1024, false);
        Application.targetFrameRate = GameConst.GameFrameRate;
        Application.runInBackground = true;
        Input.multiTouchEnabled = true;
        Screen.autorotateToPortrait = true;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        StartUp();//启动框架
        Destroy(gameObject);
    }

    /// <summary>
    /// 初始化框架，实例化游戏管理器
    /// </summary>
    private void StartUp()
    {
        Facade m_Facade = Facade.Instance;
        m_Facade.RegisterCommand(NotifyName.StartUp, typeof(CmdStartUp));
        m_Facade.RegisterCommand(NotifyName.DispatchMessage, typeof(CmdSocketDispatch));
        m_Facade.SendNotification(NotifyName.StartUp);
    }
}