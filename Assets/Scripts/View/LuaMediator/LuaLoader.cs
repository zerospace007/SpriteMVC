using System.IO;
using Framework.Utility;
using LuaInterface;
using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 集成自LuaFileUtils，重写里面的ReadFile，
    /// </summary>
    public class LuaLoader : LuaFileUtils
    {
        // Use this for initialization
        public LuaLoader()
        {
            Instance = this;
            beZip = GameConst.LuaBundleMode;
        }

        /// <summary>
        /// 添加打入Lua代码的AssetBundle
        /// </summary>
        /// <param name="bundle"></param>
        public void AddBundle(string bundleName)
        {
            string url = Util.DataPath + bundleName.ToLower();
            if (File.Exists(url))
            {
                var bytes = File.ReadAllBytes(url);
                AssetBundle bundle = AssetBundle.LoadFromMemory(bytes);
                if (null == bundle) return;
                bundleName = bundleName.Replace("lua/", "");
                AddSearchBundle(bundleName.ToLower(), bundle);
            }
        }

        /// <summary>
        /// 当LuaVM加载Lua文件的时候，这里就会被调用，
        /// 用户可以自定义加载行为，只要返回byte[]即可。
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public override byte[] ReadFile(string fileName)
        {
            return base.ReadFile(fileName);
        }
    }
}