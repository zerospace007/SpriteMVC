#region
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Framework.Core;
using Framework.Interfaces;
using Framework.Utility;
#endregion

namespace Framework
{
    /// <summary>
    /// 资源下载更新线程管理器，同时只能做下载一个任务
    /// @Author Norman Yang
    /// </summary>
    public class DownloadManager : View
    {
        #region Feilds
        private static readonly object LockObj = new object();
        private readonly Stopwatch m_StopWatch = new Stopwatch();    //计算下载时间

        private Queue<string[]> m_ThreadEvents;                     //线程事件队列
        private Thread m_Thread;                                    //当前线程
        private string m_FileSize;                                  //当前下载文件大小
        #endregion

        #region Mono Methods

        private void Awake()
        {
            MessageArray = new string[]
            {
               NotifyName.DownloadFile,
            };

            RemoveMessage(this, MessageArray);
            RegisterMessage(this, MessageArray);

            m_Thread = new Thread(OnUpdate);
            m_ThreadEvents = new Queue<string[]>();
        }

        public override void OnMessage(INotification message)
        {
            var msgName = message.Name;
            var body = message.Body;
            switch (msgName)
            {
                case NotifyName.DownloadFile:
                    AddDownloadEvent((string[])body);
                    break;
            }
        }

        private void Start()
        {
            m_Thread.Start();
        }

        /// <summary>
        /// 线程执行每毫秒调用
        /// </summary>
        private void OnUpdate()
        {
            while (true)
            {
                lock (LockObj)
                {
                    if (m_ThreadEvents.Count > 0)
                    {
                        var eventParams = m_ThreadEvents.Dequeue();
                        try
                        {
                            //下载文件
                            OnDownloadFile(eventParams);
                        }
                        catch (Exception ex)
                        {
                            Util.LogError(ex.Message);
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 应用程序退出
        /// </summary>
        private void OnDestroy()
        {
            if (m_Thread != null && m_Thread.IsAlive)
            {
                m_Thread.Abort();
                m_Thread = null;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// 添加到事件队列
        /// </summary>
        /// <param name="eventParams"></param>
        public void AddDownloadEvent(string[] eventParams)
        {
            lock (LockObj)
            {
                m_ThreadEvents.Enqueue(eventParams);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        private void OnDownloadFile(string[] eventParams)
        {
            var url = eventParams[0];
            var downFile = eventParams[1];
            m_FileSize = eventParams[2];

            using (var webClient = new WebClient())
            {
                m_StopWatch.Start();
                webClient.DownloadFileCompleted += DownloadFileCompleted;
                webClient.DownloadProgressChanged += ProgressChanged;
                webClient.DownloadFileAsync(new Uri(url), downFile);
            }
        }

        /// <summary>
        /// 下载进程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs eventArgs)
        {
            //var progressStr = string.Format("{0} MB's / {1} MB's",
            //    (eventArgs.BytesReceived/1024d/1024d).ToString("0.00"),
            //    (eventArgs.TotalBytesToReceive/1024d/1024d).ToString("0.00"));
            //var progressValue = eventArgs.ProgressPercentage / 100f;

            var progressSpeed = string.Format("{0} kb/s", (eventArgs.BytesReceived/1024d/m_StopWatch.Elapsed.TotalSeconds).ToString("0.00"));
            if (!string.IsNullOrEmpty(progressSpeed)) SendNotification(NotifyName.DownloadSpeed, progressSpeed);
        }
		
		/// <summary>
        /// 下载成功完成
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs eventArgs)
        {
            m_StopWatch.Reset();
            SendNotification(NotifyName.DownloadUpdate, m_FileSize);
        }
        #endregion
    }
}