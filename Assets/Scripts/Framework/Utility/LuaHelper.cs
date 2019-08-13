#region
using System;
using System.Reflection;
using LuaInterface;
#endregion

namespace Framework.Utility
{
    public static class LuaHelper
    {
        /// <summary>
        /// 获取类型
        /// </summary>
        /// <param name="classname"></param>
        /// <returns></returns>
        public static Type GetType(string classname)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetType(classname);
            return type;
        }

        /// <summary>
        /// Tips管理器
        /// </summary>
        /// <returns></returns>
        public static TipsManager GetTipsManager()
        {
            return Singleton.GetInstance<TipsManager>();
        }

        /// <summary>
        /// UI管理器
        /// </summary>
        public static UIManager GetUIManager()
        {
            return Singleton.GetInstance<UIManager>();
        }

        /// <summary>
        /// 网络管理器
        /// </summary>
        public static NetworkManager GetNetManager()
        {
            return Singleton.GetInstance<NetworkManager>();
        }

        /// <summary>
        /// 资源管理器
        /// </summary>
        /// <returns></returns>
        public static ResourceManager GetResManager()
        {
            return Singleton.GetInstance<ResourceManager>();
        }

        /// <summary>
        /// 获取lua管理器
        /// </summary>
        /// <returns></returns>
        public static LuaManager GetLuaManager()
        {
            return Singleton.GetInstance<LuaManager>();
        }

        /// <summary>
        /// 获取对象池管理
        /// </summary>
        /// <returns></returns>
        public static PoolManager GetPoolManager()
        {
            return Singleton.GetInstance<PoolManager>();
        }

        /// <summary>
        /// pbc/pblua函数回调
        /// </summary>
        /// <param name="data">protobuf 数据</param>
        /// <param name="function"></param>
        public static void OnCallLuaFunc(LuaByteBuffer data, LuaFunction function)
        {
            if (function != null) function.Call(data);
            Util.Log("OnCallLuaFunc length:>>" + data.buffer.Length);
        }

        /// <summary>
        /// cjson函数回调
        /// </summary>
        /// <param name="data">cjson 数据</param>
        /// <param name="function"></param>
        public static void OnJsonCallFunc(string data, LuaFunction function)
        {
            Util.Log("OnJsonCallback data:>>" + data + " lenght:>>" + data.Length);
            if (function != null) function.Call(data);
        }
    }
}