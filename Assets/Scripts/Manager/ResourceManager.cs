#region 命名空间
using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Utility;
using LuaInterface;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;
#endregion

namespace Framework
{
    public class AssetBundleInfo
    {
        public AssetBundle mAssetBundle;
        public int ReferencedCount;

        public AssetBundleInfo(AssetBundle assetBundle)
        {
            mAssetBundle = assetBundle;
            ReferencedCount = 0;
        }
    }

    public class LoadAssetRequest
    {
        public Type assetType;
        public string[] assetNames;
        public LuaFunction luaFunc;
        public Action<Object[]> sharpFunc;
    }

    public class ResourceManager : MonoBehaviour
    {
        private string[] m_AllManifest = null;
        private AssetBundleManifest m_AssetBundleManifest = null;
        private Dictionary<string, string[]> m_Dependencies = new Dictionary<string, string[]>();
        private Dictionary<string, AssetBundleInfo> m_LoadedAssetBundles = new Dictionary<string, AssetBundleInfo>();
        private Dictionary<string, List<LoadAssetRequest>> m_LoadRequests = new Dictionary<string, List<LoadAssetRequest>>();

        public string BaseDownloadingURL { get; set; } = string.Empty;              //加载基础路径

        /// <summary>
        /// Load AssetBundleManifest.
        /// </summary>
        /// <param name="manifestName"></param>
        /// <param name="initOK"></param>
        public void Initialize(string manifestName, Action initOK)
        {
            BaseDownloadingURL = Util.RelativePath + GameConst.AssetsDir + "/";
            LoadAsset<AssetBundleManifest>(manifestName, new string[] { "AssetBundleManifest" }, delegate (Object[] objs)
            {
                if (objs.Length > 0)
                {
                    m_AssetBundleManifest = objs[0] as AssetBundleManifest;
                    m_AllManifest = m_AssetBundleManifest.GetAllAssetBundles();
                }
                initOK?.Invoke();
            });
        }

        public void LoadPrefab(string abName, string assetName, Action<Object[]> func)
        {
            LoadAsset<Object>(abName, new string[] { assetName }, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, Action<Object[]> func)
        {
            LoadAsset<Object>(abName, assetNames, func);
        }

        public void LoadPrefab(string abName, string assetName, LuaFunction func)
        {
            LoadAsset<Object>(abName, new string[] { assetName }, null, func);
        }

        public void LoadPrefab(string abName, string[] assetNames, LuaFunction func)
        {
            LoadAsset<Object>(abName, assetNames, null, func);
        }

        /// <summary>
        /// 获取绝对路径
        /// </summary>
        /// <param name="abName"></param>
        /// <returns></returns>
        private string GetRealAssetPath(string abName)
        {
            if (abName.Equals(GameConst.AssetsDir)) return abName;

            abName = abName.ToLower();
            if (!abName.EndsWith(GameConst.BundleSuffix))
            {
                abName += GameConst.BundleSuffix;
            }
            if (abName.Contains("/")) return abName;

            for (int node = 0; node < m_AllManifest.Length; node++)
            {
                int index = m_AllManifest[node].LastIndexOf('/');
                string path = m_AllManifest[node].Remove(0, index + 1);    //字符串操作函数都会产生GC
                if (path.Equals(abName))
                {
                    return m_AllManifest[node];
                }
            }
            Util.LogError("GetRealAssetPath Error:>>" + abName);
            return null;
        }

        /// <summary>
        /// 载入素材
        /// </summary>
        private void LoadAsset<T>(string abName, string[] assetNames, Action<Object[]> action = null, LuaFunction func = null) where T : Object
        {
            abName = GetRealAssetPath(abName);

            LoadAssetRequest request = new LoadAssetRequest();
            request.assetType = typeof(T);
            request.assetNames = assetNames;
            request.luaFunc = func;
            request.sharpFunc = action;

            List<LoadAssetRequest> requests = null;
            if (!m_LoadRequests.TryGetValue(abName, out requests))
            {
                requests = new List<LoadAssetRequest>();
                requests.Add(request);
                m_LoadRequests.Add(abName, requests);
                StartCoroutine(OnLoadAsset<T>(abName));
            }
            else
            {
                requests.Add(request);
                //m_LoadRequests.Add(abName, requests);
            }
        }

        /// <summary>
        /// 加载AB协程
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="abName"></param>
        /// <returns></returns>
        private IEnumerator OnLoadAsset<T>(string abName) where T : Object
        {
            AssetBundleInfo bundleInfo = GetLoadedAssetBundle(abName);
            if (bundleInfo == null)
            {
                yield return StartCoroutine(OnLoadAssetBundle(abName, typeof(T)));

                bundleInfo = GetLoadedAssetBundle(abName);
                if (bundleInfo == null)
                {
                    m_LoadRequests.Remove(abName);
                    Debug.LogError("OnLoadAsset--->>>" + abName);
                    yield break;
                }
            }
            List<LoadAssetRequest> list = null;
            if (!m_LoadRequests.TryGetValue(abName, out list))
            {
                m_LoadRequests.Remove(abName);
                yield break;
            }
            for (int node = 0, count = list.Count; node < count; node++)
            {
                string[] assetNames = list[node].assetNames;
                List<Object> result = new List<Object>();

                AssetBundle assetBundle = bundleInfo.mAssetBundle;
                for (int repo = 0; repo < assetNames.Length; repo++)
                {
                    string assetPath = assetNames[repo];
                    AssetBundleRequest request = assetBundle.LoadAssetAsync(assetPath, list[node].assetType);
                    yield return request;
                    result.Add(request.asset);
                }
                if (list[node].sharpFunc != null)
                {
                    list[node].sharpFunc(result.ToArray());
                    list[node].sharpFunc = null;
                }
                if (list[node].luaFunc != null)
                {
                    list[node].luaFunc.Call((object)result.ToArray());
                    list[node].luaFunc.Dispose();
                    list[node].luaFunc = null;
                }
                bundleInfo.ReferencedCount++;
            }
            m_LoadRequests.Remove(abName);
        }

        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private IEnumerator OnLoadAssetBundle(string abName, Type type)
        {
            string url = BaseDownloadingURL + abName;

            Util.LogWarning(url);
            UnityWebRequest download = null;
            if (type == typeof(AssetBundleManifest))
            {
                download = UnityWebRequestAssetBundle.GetAssetBundle(url);
            }
            else
            {
                string[] dependencies = m_AssetBundleManifest.GetAllDependencies(abName);
                if (dependencies.Length > 0)
                {
                    m_Dependencies.Add(abName, dependencies);
                    for (int i = 0; i < dependencies.Length; i++)
                    {
                        string depName = dependencies[i];
                        AssetBundleInfo bundleInfo = null;
                        if (m_LoadedAssetBundles.TryGetValue(depName, out bundleInfo))
                        {
                            bundleInfo.ReferencedCount++;
                        }
                        else if (!m_LoadRequests.ContainsKey(depName))
                        {
                            yield return StartCoroutine(OnLoadAssetBundle(depName, type));
                        }
                    }
                }
                download = UnityWebRequestAssetBundle.GetAssetBundle(url, m_AssetBundleManifest.GetAssetBundleHash(abName), 0);
            }
            yield return download.SendWebRequest();
            AssetBundle assetObj = DownloadHandlerAssetBundle.GetContent(download);
            if (assetObj != null)
            {
                m_LoadedAssetBundles.Add(abName, new AssetBundleInfo(assetObj));
            }
        }

        /// <summary>
        /// 获取AB资源信息
        /// </summary>
        /// <param name="abName">AB名称</param>
        /// <returns></returns>
        private AssetBundleInfo GetLoadedAssetBundle(string abName)
        {
            AssetBundleInfo bundle = null;
            m_LoadedAssetBundles.TryGetValue(abName, out bundle);
            if (bundle == null) return null;

            // No dependencies are recorded, only the bundle itself is required.
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies)) return bundle;

            // Make sure all dependencies are loaded
            foreach (var dependency in dependencies)
            {
                AssetBundleInfo dependentBundle;
                m_LoadedAssetBundles.TryGetValue(dependency, out dependentBundle);
                if (dependentBundle == null) return null;
            }
            return bundle;
        }

        /// <summary>
        /// 此函数交给外部卸载专用，自己调整是否需要彻底清除AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        public void UnloadAssetBundle(string abName, bool isThorough = false)
        {
            abName = GetRealAssetPath(abName);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory before unloading " + abName);
            UnloadAssetBundleInternal(abName, isThorough);
            UnloadDependencies(abName, isThorough);
            Debug.Log(m_LoadedAssetBundles.Count + " assetbundle(s) in memory after unloading " + abName);
        }

        /// <summary>
        /// 卸载依赖AB
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="isThorough"></param>
        private void UnloadDependencies(string abName, bool isThorough)
        {
            string[] dependencies = null;
            if (!m_Dependencies.TryGetValue(abName, out dependencies)) return;

            // Loop dependencies.
            foreach (var dependency in dependencies)
            {
                UnloadAssetBundleInternal(dependency, isThorough);
            }
            m_Dependencies.Remove(abName);
        }

        private void UnloadAssetBundleInternal(string abName, bool isThorough)
        {
            AssetBundleInfo bundle = GetLoadedAssetBundle(abName);
            if (bundle == null) return;

            if (--bundle.ReferencedCount <= 0)
            {
                if (m_LoadRequests.ContainsKey(abName))
                {
                    return;     //如果当前AB处于Async Loading过程中，卸载会崩溃，只减去引用计数即可
                }
                bundle.mAssetBundle.Unload(isThorough);
                m_LoadedAssetBundles.Remove(abName);
                Debug.Log(abName + " has been unloaded successfully");
            }
        }
    }
}
