/// <summary>
/// 广播通知消息
/// @Author NormanYang
/// </summary>
public class NotifyName
{
    /// <summary>
    /// Controller层消息通知
    /// </summary>
    public const string StartUp = "StartUp";                            //启动框架
    public const string DispatchMessage = "DispatchMessage";            // 派发网络消息

    /// <summary>
    /// View层消息通知
    /// </summary>
    public const string UpdateMessage = "UpdateMessage";                //更新消息
    public const string UnpackOrDownload = "UnpackOrDownload";          //当前是解包还是下载
    public const string UnpackTotolCount = "UnpackTotolCount";          //解包总数
    public const string UnpackUpdate = "UnpackUpdate";                  //解包一个完成

    public const string DownloadFile = "DownloadFile";                  //下载某文件
    public const string DownloadTotolBytes = "DownloadTotolBytes";      //需要下载总大小
    public const string DownloadUpdate = "DownloadUpdate";              //下载一个完成
    public const string DownloadSpeed = "DownloadSpeed";                //更新下载速度
    public const string ResourceInited = "ResourceInited";              //资源准备完成
}
