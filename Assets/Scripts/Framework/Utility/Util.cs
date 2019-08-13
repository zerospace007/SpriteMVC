#region
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif
#endregion

namespace Framework.Utility
{
    public class Util
    {
        /// <summary>
        /// 取得数据存放目录（persistentDataPath）
        /// </summary>
        public static string DataPath
        {
            get
            {
                var gameName = GameConst.GameName;
                if (Application.isMobilePlatform)
                {
                    return Application.persistentDataPath + "/" + gameName + "/";
                }
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    return Application.streamingAssetsPath + "/";
                }
                if (GameConst.DebugMode && Application.isEditor)
                {
                    return Application.streamingAssetsPath + "/";
                }
                if (Application.platform == RuntimePlatform.OSXEditor)
                {
                    var indexOf = Application.dataPath.LastIndexOf('/');
                    return Application.dataPath.Substring(0, indexOf + 1) + gameName + "/";
                }
                return "D:/" + gameName + "/";
            }
        }

        /// <summary>
        /// 应用程序内容路径（StreamingAssets）
        /// </summary>
        public static string ContentPath
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        return "jar:file://" + Application.dataPath + "!/assets/";
                    case RuntimePlatform.IPhonePlayer:
                        return Application.dataPath + "/Raw/";
                    default:
                        return Application.dataPath + "/StreamingAssets/";
                }
            }
        }

        /// <summary>
        /// AssetBundle加在路径（移动端persistentDataPath， PC端streamingAssetsPath）
        /// </summary>
        public static string RelativePath
        {
            get
            {
                if (GameConst.DebugMode && Application.isEditor)
                    return "file://" + Application.streamingAssetsPath + "/";
                else if (Application.isMobilePlatform || Application.isConsolePlatform || !GameConst.DebugMode)
                    return "file://" + DataPath;
                else // For standalone player.
                    return "file://" + Application.streamingAssetsPath + "/";
            }
        }

        /// <summary>
        /// 下载更新网址，各个平台不同，下载更新的路径不同
        /// </summary>
        public static string WebUrlPlatform
        {
            get
            {
                var url = GameConst.WebUrl + GameConst.VersionNumber;
                switch (Application.platform)
                {
                    case RuntimePlatform.IPhonePlayer:
                        url += "/iOS/";
                        break;
                    case RuntimePlatform.Android:
                        url += "/Android/";
                        break;
                    default:
                        url += "/StandaloneWindows/";
                        break;
                }
                return url;
            }
        }

        /// <summary>
        /// 网络可用
        /// </summary>
        public static bool NetAvailable
        {
            get { return Application.internetReachability != NetworkReachability.NotReachable; }
        }

        /// <summary>
        /// 是否是无线
        /// </summary>
        public static bool IsWifi
        {
            get { return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork; }
        }

        /// <summary>
        /// 是不是苹果平台
        /// </summary>
        /// <returns></returns>
        public static bool IsApplePlatform
        {
            get
            {
                return Application.platform == RuntimePlatform.IPhonePlayer ||
                       Application.platform == RuntimePlatform.OSXEditor ||
                       Application.platform == RuntimePlatform.OSXPlayer;
            }
        }

        /// <summary>
        /// 执行加载赋值
        /// </summary>
        /// <param name="action"></param>
        /// <param name="spriteAtlas"></param>
        /// <returns></returns>
        public static bool DoAction(Action<SpriteAtlas> action, SpriteAtlas spriteAtlas)
        {
            action(spriteAtlas);
            return true;
        }

        /// <summary>
        /// 世界坐标转换为canvas屏幕坐标
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static Vector2 WorldToScreenPoint(Camera camera, Vector3 position)
        {
            var pos = RectTransformUtility.WorldToScreenPoint(camera, position);
            return pos;
        }

        /// <summary>
        /// 判断游戏对象是否为空
        /// </summary>
        /// <param name="gameObj"></param>
        /// <returns></returns>
        public static bool IsNotNull(GameObject gameObj)
        {
            return null != gameObj;
        }

        /// <summary>
        /// 是否出界（例如是否在（0，0）（600，600）的范围内）
        /// </summary>
        /// <param name="pointPos"></param>
        /// <param name="minVec"></param>
        /// <param name="MaxVec"></param>
        /// <returns></returns>
        public static bool IsOutofBounds(Vector2 pointPos, Vector2 minVec, Vector2 MaxVec)
        {
            return pointPos.IsOutofBounds(minVec, MaxVec);
        }

        /// <summary>
        /// 拆箱int型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static int Int(object obj)
        {
            return Convert.ToInt32(obj);
        }

        /// <summary>
        /// 拆箱 float型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static float Float(object obj)
        {
            return (float)Math.Round(Convert.ToSingle(obj), 2);
        }

        /// <summary>
        /// 拆箱 Long型
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static long Long(object obj)
        {
            return Convert.ToInt64(obj);
        }

        /// <summary>
        /// 最大最小值之间取随机（float）
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float Random(float min, float max)
        {
            return UnityEngine.Random.Range(min, max);
        }

        /// <summary>
        ///当前毫秒数
        /// </summary>
        /// <returns></returns>
        public static long GetTime()
        {
            var timeSpan = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
            return (long)timeSpan.TotalMilliseconds;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public static T AddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;
            var comps = gameObject.GetComponents<T>();
            for (int node = 0, length = comps.Length; node < length; node++)
            {
                if (comps[node] != null) Object.Destroy(comps[node]);
            }
            return gameObject.AddComponent<T>();
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        public static T AddComponent<T>(Transform transform) where T : Component
        {
            return AddComponent<T>(transform.gameObject);
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject FindPeer(GameObject gameObjectBro, string subNode)
        {
            return FindPeer(gameObjectBro.transform, subNode);
        }

        /// <summary>
        /// 取平级对象
        /// </summary>
        public static GameObject FindPeer(Transform transformBro, string subnode)
        {
            var transform = transformBro.parent.Find(subnode);
            return transform?.gameObject;
        }

        /// <summary>
        /// 销毁某游戏对象上的组件
        /// </summary>
        /// <param name="gameObject">游戏显示对象</param>
        /// <param name="className">类名</param>
        public static void DestroyComponent(GameObject gameObject, string className)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetType(className);

            Component component = null;
            if (type != null)
            {
                component = gameObject.GetComponent(type);
            }
            Object.Destroy(component);
        }

        /// <summary>
        /// Base64编码
        /// </summary>
        public static string Encode(string message)
        {
            var bytes = Encoding.GetEncoding("utf-8").GetBytes(message);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Base64解码
        /// </summary>
        public static string Decode(string message)
        {
            var bytes = Convert.FromBase64String(message);
            return Encoding.GetEncoding("utf-8").GetString(bytes);
        }

        /// <summary>
        /// 判断数字
        /// </summary>
        public static bool IsNumeric(string str)
        {
            if (string.IsNullOrEmpty(str)) return false;
            foreach (var tchar in str)
            {
                if (!char.IsNumber(tchar))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// HashToMD5Hex
        /// </summary>
        public static string HashToMd5Hex(string sourceStr)
        {
            var bytes = Encoding.UTF8.GetBytes(sourceStr);
            using (var md5 = new MD5CryptoServiceProvider())
            {
                var result = md5.ComputeHash(bytes);
                var builder = new StringBuilder();
                foreach (var byteCode in result)
                    builder.Append(byteCode.ToString("x2"));
                return builder.ToString();
            }
        }

        /// <summary>
        /// 计算字符串的MD5值
        /// </summary>
        public static string Md5(string source)
        {
            var md5 = new MD5CryptoServiceProvider();
            var data = Encoding.UTF8.GetBytes(source);
            var md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            var destString = "";
            foreach (var value in md5Data)
            {
                destString += Convert.ToString(value, 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString;
        }

        /// <summary>
        /// 计算文件的MD5值
        /// </summary>
        public static string GetMd5Code(string file)
        {
            try
            {
                var fileStream = new FileStream(file, FileMode.Open);
                var md5 = new MD5CryptoServiceProvider();
                var retVal = md5.ComputeHash(fileStream);
                fileStream.Close();

                var stringBuilder = new StringBuilder();
                for (int node = 0, length = retVal.Length; node < length; node++)
                {
                    stringBuilder.Append(retVal[node].ToString("x2"));
                }
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5Code() fail, error:" + ex.Message);
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string FileSize(string file)
        {
            var fileInfo = new FileInfo(file);
            return fileInfo.Length.ToString();
        }

        /// <summary>
        /// 清除所有子节点
        /// </summary>
        public static void ClearChild(Transform transform)
        {
            if (transform == null) return;
            for (var node = transform.childCount - 1; node >= 0; node--)
            {
                Object.Destroy(transform.GetChild(node).gameObject);
            }
        }

        /// <summary>
        /// 清理内存
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            Resources.UnloadUnusedAssets();
            Singleton.GetInstance<LuaManager>().LuaGC();
        }

        /// <summary>
        /// 是否为数字
        /// </summary>
        public static bool IsNumber(string strNumber)
        {
            var regex = new Regex("[^0-9]");
            return !regex.IsMatch(strNumber);
        }

        /// <summary>
        /// log日志
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args"></param>
        public static void Log(string str, params object[] args)
        {
            if (args != null && args.Length > 0) str = string.Format(str, args);
            Debug.Log(str);
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args"></param>
        public static void LogWarning(string str, params object[] args)
        {
            if (args != null && args.Length > 0) str = string.Format(str, args);
            Debug.LogWarning(str);
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="str"></param>
        /// <param name="args"></param>
        public static void LogError(string str, params object[] args)
        {
            if (args != null && args.Length > 0) str = string.Format(str, args);
            Debug.LogError(str);
        }

        /// <summary>
        /// 设置父子关系
        /// </summary>
        /// <param name="parentObj"></param>
        /// <param name="childObj"></param>
        public static void SetParent(Transform parentObj, Transform childObj)
        {
            var layerName = LayerSetting.Default;
            if (null != parentObj)
            {
                childObj.SetParent(parentObj);
                layerName = LayerMask.LayerToName(parentObj.gameObject.layer);
            }
            LayerSetting.ChangeLayer(childObj, layerName);
        }

        /// <summary>
        /// 设置父子关系
        /// </summary>
        /// <param name="parentObj"></param>
        /// <param name="childObj"></param>
        public static void SetParent(GameObject parentObj, GameObject childObj)
        {
            SetParent(parentObj.transform, childObj.transform);
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="gameObject">游戏显示对象</param>
        /// <param name="assembly">命名空间</param>
        /// <param name="className">类名</param>
        /// <returns></returns>
        public static Component AddComponent(GameObject gameObject, string assembly, string className)
        {
            var asmb = Assembly.Load(assembly);
            var type = asmb.GetType(assembly + "." + className);
            return gameObject.AddComponent(type);
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="gameObject">游戏显示对象</param>
        /// <param name="typeName">类名</param>
        /// <returns></returns>
        public static Component AddComponent(GameObject gameObject, string typeName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var type = assembly.GetType(typeName);
            return gameObject.AddComponent(type);
        }

        /// <summary>
        /// 载入prefab
        /// </summary>
        /// <param name="prefabName">预设名称</param>
        public static GameObject Load(string prefabName)
        {
            return Resources.Load(prefabName, typeof(GameObject)) as GameObject;
        }

        /// <summary>
        /// 卸载prefab
        /// </summary>
        /// <param name="uObject"></param>
        public static void UnLoad(Object uObject)
        {
            Resources.UnloadAsset(uObject);
        }

        /// <summary>
        /// 防止初学者不按步骤来操作
        /// </summary>
        /// <returns></returns>
        private static int CheckRuntimeFile()
        {
            if (!Application.isEditor) return 0;
            var streamDir = Application.dataPath + "/StreamingAssets/";
            if (!Directory.Exists(streamDir))
            {
                return -1;
            }
            var files = Directory.GetFiles(streamDir);
            if (files.Length == 0) return -1;

            var sourceDir = Application.dataPath + "/ToLua/Source/Generate/";
            if (!Directory.Exists(sourceDir))
            {
                return -2;
            }
            files = Directory.GetFiles(sourceDir);
            if (files.Length == 0) return -2;
            return 0;
        }

        /// <summary>
        /// 检查运行环境
        /// </summary>
        public static bool CheckEnvironment()
        {
#if UNITY_EDITOR
            var resultId = CheckRuntimeFile();
            switch (resultId)
            {
                case -1:
                    Debug.LogError("没有找到框架所需要的资源，单击Game菜单下Build xxx Resource生成！！");
                    EditorApplication.isPlaying = false;
                    return false;
                case -2:
                    Debug.LogError("没有找到Wrap脚本缓存，单击Lua菜单下Gen Lua Wrap Files生成脚本！！");
                    EditorApplication.isPlaying = false;
                    return false;
            }
#endif
            return true;
        }

        /// <summary>
        /// 执行Lua方法
        /// </summary>
        /// <param name="module"></param>
        /// <param name="func"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static object CallMethod(string module, string func, params object[] args)
        {
            return Singleton.GetInstance<LuaManager>().CallFunction<object>(module + "." + func, args);
        }

        /// <summary>
        /// 给Lua字段赋值
        /// </summary>
        /// <param name="module"></param>
        /// <param name="feild"></param>
        /// <param name="feildValue"></param>
        public static void SetLuaFeild(string module, string feild, object feildValue)
        {
            var luaMgr = Singleton.GetInstance<LuaManager>();
            if (luaMgr.mLuaState == null) return;
            luaMgr.mLuaState[module + "." + feild] = feildValue;
        }

        /// <summary>
        /// 获取Lua字段
        /// </summary>
        /// <param name="module"></param>
        /// <param name="feild"></param>
        /// <returns></returns>
        public static object GetLuaFeild(string module, string feild)
        {
            var luaMgr = Singleton.GetInstance<LuaManager>();
            if (luaMgr.mLuaState == null) return null;
            return luaMgr.mLuaState[module + "." + feild];
        }

        public static void SetRect(RectTransform rect,float x,float y)
        {
           rect.offsetMin = rect.offsetMin + new Vector2(x,y);
        }
        public static void SetRect(RectTransform rect,Vector2 offsetMin, float x, float y)
        {
            rect.offsetMin = offsetMin + new Vector2(x, y);
        }
        public static void SetRectOffMax(RectTransform rect, Vector2 offsetMax, float x, float y)
        {
            rect.offsetMax = offsetMax + new Vector2(x, y);
        }
    }
}