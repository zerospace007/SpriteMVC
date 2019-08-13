#region 命名空间
using System.Collections.Generic;
using Framework.Core;
using Framework.Interfaces;
using Framework.Utility;
#endregion

/// <summary>
/// 网络Socket派发协议
/// Author Norman
/// </summary>
public class CmdSocketDispatch : SimpleCommand
{
    public override void Execute(INotification message)
    {
        var data = message.Body;
        if (data == null) return;
        var buffer = (KeyValuePair<int, string>) data;
        Util.CallMethod("Network", "OnSocket", buffer.Key, buffer.Value);

    }
}