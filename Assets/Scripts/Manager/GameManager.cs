#region using
using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using Framework.Core;
using Framework.Interfaces;
using Framework.Utility;
using UnityEngine;
using UnityEngine.Networking;
#endregion

namespace Framework
{
    /// <summary>
    /// 游戏解包下载管理
    /// </summary>
    public class GameManager : View
    {
        private bool IsDownOk;              //单个文件是否下载完成
        /// 初始化游戏管理器
        /// </summary>
        private void Awake()
        {
            MessageArray = new string[]
            {
                NotifyName.ResourceInited,
                NotifyName.DownloadUpdate,
            };

            RemoveMessage(this, MessageArray);
            RegisterMessage(this, MessageArray);

            DontDestroyOnLoad(gameObject);              //防止销毁自己
            CheckExtractResource();                     //释放资源
        }

        public override void OnMessage(INotification message)
        {
            var msgName = message.Name;
            var body = message.Body;
            switch (msgName)
            {
                case NotifyName.ResourceInited:
                    OnResourceInited();
                    break;
                case NotifyName.DownloadUpdate:
                    IsDownOk = true;
                    break;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void CheckExtractResource()
        {
            var dataPath = Util.DataPath;               //数据目录
            var isExists = Directory.Exists(dataPath) && File.Exists(dataPath + GameConst.VersionBytes);
            if (isExists || GameConst.DebugMode)
            {
                //文件已经解压过了，自己可添加检查文件列表逻辑
                StartCoroutine(OnUpdateResource());
                return;
            }
            StartCoroutine(OnExtractResource());        //启动释放协程
        }

        /// <summary>
        /// 从StreamingAssets目录释放解包到数据目录
        /// </summary>
        /// <returns></returns>
        private IEnumerator OnExtractResource()
        {
            SendNotification(NotifyName.UnpackOrDownload, false);
            var resPath = Util.ContentPath;             //游戏包资源目录
            var dataPath = Util.DataPath;               //数据目录
            var infile = resPath + GameConst.VersionBytes;
            var outfile = dataPath + GameConst.VersionBytes;

            Util.Log("当下数据目录：" + dataPath);

            if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
            Directory.CreateDirectory(dataPath);
            if (File.Exists(outfile)) File.Delete(outfile);

            Util.Log("正在解包文件:>" + GameConst.VersionBytes);

            if (Application.platform == RuntimePlatform.Android)
            {
                var webRequest = UnityWebRequest.Get(infile);
                yield return webRequest.SendWebRequest();
                File.WriteAllBytes(outfile, webRequest.downloadHandler.data);
            }
            else File.Copy(infile, outfile, true);
            yield return new WaitForEndOfFrame();

            //释放所有文件到数据目录
            var unpackCurrentCount = 0;
            var fileDatas = File.ReadAllLines(outfile);
            SendNotification(NotifyName.UnpackTotolCount, fileDatas.Length);
            foreach (var fileData in fileDatas)
            {
                var valueArr = fileData.Split('|');
                var fileName = valueArr[0];
                infile = resPath + fileName;
                outfile = dataPath + fileName;

                Util.Log("正在解包文件:>" + fileName);

                var directory = Path.GetDirectoryName(outfile);
                if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                if (Application.platform == RuntimePlatform.Android)
                {
                    var webRequest = UnityWebRequest.Get(infile);
                    yield return webRequest.SendWebRequest();
                    File.WriteAllBytes(outfile, webRequest.downloadHandler.data);
                }
                else
                {
                    if (File.Exists(outfile)) File.Delete(outfile);
                    File.Copy(infile, outfile, true);
                }
                yield return new WaitForEndOfFrame();
                SendNotification(NotifyName.UnpackUpdate, ++unpackCurrentCount);
            }
            Util.Log("解包完成!!!");
            yield return new WaitForSeconds(0.1f);

            //释放完成，开始启动更新资源
            StartCoroutine(OnUpdateResource());
        }

        /// <summary>
        /// 启动更新下载，从Web服务器下载到本机数据目录，此处可启动线程下载更新
        /// </summary>
        private IEnumerator OnUpdateResource()
        {
            SendNotification(NotifyName.UnpackOrDownload, true);

            if (!GameConst.UpdateMode)
            {
                OnResourceInited();
                yield break;
            }
            var dataPath = Util.DataPath;               //数据目录
            var webUrlPlatform = Util.WebUrlPlatform;   //下载更新目录
            var random = DateTime.Now.ToString("yyyymmddhhmmss");
            var versionUrl = webUrlPlatform + GameConst.VersionBytes + "?v=" + random;
            Util.LogWarning("LoadUpdate---->>>" + versionUrl);

            var webRequest = UnityWebRequest.Get(versionUrl);
            yield return webRequest.SendWebRequest();
            if (webRequest.error != null)
            {
                OnUpdateFailed(string.Empty);
                yield break;
            }
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }
            File.WriteAllBytes(dataPath + GameConst.VersionBytes, webRequest.downloadHandler.data);

            var filesText = webRequest.downloadHandler.text;
            var fileDatas = Regex.Split(filesText, "\r\n", RegexOptions.IgnoreCase);

            var totalBytes = GetTotalBytes(dataPath, fileDatas);
            SendNotification(NotifyName.DownloadTotolBytes, totalBytes);

            foreach (var file in fileDatas)
            {
                if (string.IsNullOrEmpty(file)) continue;
                var valueArr = file.Split('|');
                var fileName = valueArr[0];
                var fileSize = valueArr[2];
                var localfile = (dataPath + fileName).Trim();
                var path = Path.GetDirectoryName(localfile);
                if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
                var fileUrl = webUrlPlatform + fileName + "?v=" + random;
                var isCanUpdate = !File.Exists(localfile);
                if (!isCanUpdate)
                {
                    var remoteMd5 = valueArr[1].Trim();
                    var localMd5 = Util.GetMd5Code(localfile);
                    isCanUpdate = !remoteMd5.Equals(localMd5);
                    if (isCanUpdate) File.Delete(localfile);
                }
                if (!isCanUpdate) continue;
                //本地缺少文件
                Util.Log("Downloading>>" + fileUrl);

                //这里都是资源文件，用线程下载
                BeginDownload(fileUrl, localfile, fileSize);
                while (!IsDownOk)
                {
                    yield return new WaitForEndOfFrame();
                }
            }
            yield return new WaitForEndOfFrame();
            Util.Log("更新完成!!");
        }

        /// <summary>
        /// 获取当前需要更新的总字节数
        /// </summary>
        /// <param name="dataPath"></param>
        /// <param name="fileDatas"></param>
        /// <returns></returns>
        private float GetTotalBytes(string dataPath, string[] fileDatas)
        {
            var totalBytes = 0f;
            foreach (var file in fileDatas)
            {
                if (string.IsNullOrEmpty(file)) continue;
                var valueArr = file.Split('|');
                var fileName = valueArr[0];
                var fileSize = valueArr[2];
                var localfile = (dataPath + fileName).Trim();
                var path = Path.GetDirectoryName(localfile);
                if (path != null && !Directory.Exists(path)) Directory.CreateDirectory(path);
                var isCanUpdate = !File.Exists(localfile);
                if (!isCanUpdate)
                {
                    var remoteMd5 = valueArr[1].Trim();
                    var localMd5 = Util.GetMd5Code(localfile);
                    isCanUpdate = !remoteMd5.Equals(localMd5);
                }
                if (isCanUpdate) totalBytes += Convert.ToSingle(fileSize);
            }
            return totalBytes;
        }

        /// <summary>       
        /// 开始添加下载单个资源文件
        /// </summary>
        /// <param name="url"></param>
        /// <param name="file"></param>
        /// <param name="fileSize"></param>
        private void BeginDownload(string url, string file, string fileSize)
        {
            //下载单个资源文件参数
            IsDownOk = false;
            var param = new string[] { url, file, fileSize };
            SendNotification(NotifyName.DownloadFile, param);
        }

        /// <summary>
        /// 资源初始化
        /// </summary>
        public void OnResourceInited()
        {
            var doInitialize = new Action(OnInitialize);
            Singleton.GetInstance<ResourceManager>().Initialize(GameConst.AssetsDir, doInitialize);
        }

        /// <summary>
        /// Lua初始化及入口
        /// </summary>
        private void OnInitialize()
        {
            Util.Log("Initialize OK!!!");
            LuaManager.InitStart();
            LuaManager.DoFile("Logic/GameManager");                         //加载游戏
            Util.CallMethod("GameManager", "OnInit");                       //初始化Lua FrameWork并初始化配置管理等
            UIManager.HideUI(DownloadView.Name);                            //关闭加载界面
        }

        /// <summary>
        /// 文件更新失败
        /// </summary>
        /// <param name="file">文件名称</param>
        private void OnUpdateFailed(string file)
        {
            Util.LogWarning("更新失败!>" + file);
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        private void OnDestroy()
        {
            LuaManager.Close();
            Debug.Log("~GameManager was destroyed");
        }
    }
}