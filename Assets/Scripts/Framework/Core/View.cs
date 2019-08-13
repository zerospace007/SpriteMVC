#region
using System.Collections.Generic;
using Framework.Interfaces;
using Framework.Utility;
using UnityEngine;
#endregion

namespace Framework.Core
{
    /// <summary>
    /// View显示控制
    /// </summary>
    public class View : MonoBehaviour, IView
    {
        #region Feilds and Properties

        private GameFacade Facade = GameFacade.Instance;                //外观管理类

        private ResourceManager _resourceManager;                       //资源加载管理器
        private LuaManager _luaManager;                                 //Lua脚本管理
        private UIManager _uiManager;                                   //界面管理器

        protected List<string> MessageList { get; set; }                //监听的消息列表
        protected string[] MessageArray { get; set; }                   //监听的消息数组

        /// <summary>
        /// 资源加载管理器
        /// </summary>
        protected ResourceManager ResourceManager => _resourceManager ?? (_resourceManager = Singleton.GetInstance<ResourceManager>());

        /// <summary>
        /// LuaInterface C#、lua间调用管理
        /// </summary>
        protected LuaManager LuaManager => _luaManager ?? (_luaManager = Singleton.GetInstance<LuaManager>());

        /// <summary>
        /// 界面管理器
        /// </summary>
        protected UIManager UIManager => _uiManager ?? (_uiManager = Singleton.GetInstance<UIManager>());

        #endregion

        #region Methods

        /// <summary>
        /// 消息接收执行
        /// </summary>
        /// <param name="message">消息名称</param>
        public virtual void OnMessage(INotification message)
        {
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="view"></param>
        /// <param name="messages"></param>
        protected void RegisterMessage(IView view, List<string> messages)
        {
            if (messages == null || messages.Count == 0) return;
            Controller.Instance.RegisterViewCommand(view, messages.ToArray());
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="view"></param>
        /// <param name="messages"></param>
        protected void RemoveMessage(IView view, List<string> messages)
        {
            if (messages == null || messages.Count == 0) return;
            Controller.Instance.RemoveViewCommand(view, messages.ToArray());
        }

        /// <summary>
        /// 注册消息
        /// </summary>
        /// <param name="view"></param>
        /// <param name="messages"></param>
        protected void RegisterMessage(IView view, string[] messages)
        {
            if (messages == null || messages.Length == 0) return;
            Controller.Instance.RegisterViewCommand(view, messages);
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="view"></param>
        /// <param name="messages"></param>
        protected void RemoveMessage(IView view, string[] messages)
        {
            if (messages == null || messages.Length == 0) return;
            Controller.Instance.RemoveViewCommand(view, messages);
        }

        /// <summary>
        /// 发送消息体并执行
        /// </summary>
        /// <param name="notifyName">消息名称</param>
        /// <param name="body">消息内容</param>
        public void SendNotification(string notifyName, object body = null)
        {
            Facade.SendNotification(notifyName, body);
        }

        #endregion
    }
}