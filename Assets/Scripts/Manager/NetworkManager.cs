#region 命名空间
using System.Collections.Generic;
using Framework.Core;
using Framework.Network;
using Framework.Utility;
using UnityEngine;
#endregion

namespace Framework
{
    /// <summary>
    /// 网络管理
    /// @Author Norman Yang
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        /// <summary>
        /// Socket客户端
        /// </summary>
        private SocketClient m_SocketClient;

        /// <summary>
        /// 锁定对象
        /// </summary>
        private static readonly object m_lockObject = new object();
        /// <summary>
        /// 消息队列
        /// </summary>
        private static Queue<KeyValuePair<int, string>> EventQuene = new Queue<KeyValuePair<int, string>>();

        /// <summary>
        /// 登录客户端
        /// </summary>
        private SocketClient SocketClient
        {
            get{ return m_SocketClient == null ? m_SocketClient = new SocketClient(): m_SocketClient;}
        }

        void Awake()
        {
            Init();
        }

        void Init()
        {
            //登录与游戏客户端初始化
            SocketClient.OnRegister();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void OnInit()
        {
            CallMethod("Start");
        }

        /// <summary>
        /// 卸载
        /// </summary>
        public void Unload()
        {
            CallMethod("Unload");
        }

        /// <summary>
        /// 调用Lua Network管理方法
        /// </summary>
        private object CallMethod(string func, params object[] args)
        {
            return Util.CallMethod("Network", func, args);
        }

        /// <summary>
        /// 网络消息分发
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="data"></param>
        public static void AddEvent(int eventID, string data)
        {
            lock (m_lockObject)
            {
                EventQuene.Enqueue(new KeyValuePair<int, string>(eventID, data));
            }
        }

        /// <summary>
        /// 交给Command，这里不想关心发给谁。
        /// </summary>
        private void Update()
        {
            if (EventQuene.Count <= 0) return;
            while (EventQuene.Count > 0)
            {
                var eventData = EventQuene.Dequeue();
                Facade.Instance.SendNotification(NotifyName.DispatchMessage, eventData);
            }
        }

        /// <summary>
        /// 发送连接服务器请求
        /// </summary>
        /// <param name="socketIP"></param>
        /// <param name="socketPort"></param>
        public void SendConnect(string socketIP, int socketPort)
        {
            SocketClient.SendConnect(socketIP, socketPort);
        }

        /// <summary>
        /// 发送协议内容（此游戏通过Json协议，不再通过buffer封装）
        /// </summary>
        private void SendNetMessage(ByteBuffer buffer)
        {
            SocketClient.SendMessage(buffer);
        }

        /// <summary>
        /// 发送协议内容
        /// </summary>
        /// <param name="data"></param>
        public void SendNetMessage(string data)
        {
            SocketClient.SendMessage(data);
        }

        /// <summary>
        /// 组件销毁
        /// </summary>
        void OnDestroy()
        {
            SocketClient.OnRemove();
            Util.Log("~NetworkManager was destroy");
        }
    }
}