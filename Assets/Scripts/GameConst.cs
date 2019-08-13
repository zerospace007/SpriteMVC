#region
using UnityEngine;
#endregion

public class GameConst
{
    #region Static Feilds
                                                                //注意 打包成android时 要改为false  让其解包
    public static bool DebugMode = true;                        //调试模式-用于内部测试 true表示走本地StreamingAssets
    public static bool LogMode = false;                         //Log日志显示模式

    /// <summary>
    /// 如果开启更新模式，前提必须启动框架自带服务器端。
    /// 否则就需要自己将StreamingAssets里面的所有内容
    /// 复制到自己的Webserver上面，并修改下面的WebUrl。
    /// </summary>
    public static bool UpdateMode = false;                      //更新模式-默认关闭 
    public static bool LuaByteMode = false;                     //Lua字节码模式-默认关闭 
    public static bool LuaBundleMode = false;                   //Lua代码AssetBundle模式

    public const int TimerInterval = 1;
    public const int GameFrameRate = 60;                        //游戏帧频
    public const string GameName = "expedition";                //应用程序名称
    public const string AssetsDir = "assets_dir";               //manifest
    public const string BundleSuffix = "";                      //素材扩展名
    public const string WebUrl = "http://139.196.174.22/web-mobile/slg/resource/";      //资源更新地址
    public const string VersionNumber = "1.0.0";                //版本号
    public const string VersionBytes = "version.bytes";         //版本文件列表
    #endregion
}