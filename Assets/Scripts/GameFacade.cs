#region
using Framework.Core;
#endregion

/// <summary>
/// 游戏外观初始化入口
/// </summary>
public class GameFacade : Facade
{
    protected override void InitFramework()
    {
        base.InitFramework();
        RegisterCommand(NotifyName.StartUp, typeof(CmdStartUp));
        RegisterCommand(NotifyName.DispatchMessage, typeof(CmdSocketDispatch));
    }

    /// <summary>
    /// 启动框架
    /// </summary>
    public void StartUp()
    {
        SendNotification(NotifyName.StartUp);
        RemoveCommand(NotifyName.StartUp);
    }
}

