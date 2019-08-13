#region

using System;
using Framework.Interfaces;
using Framework.Utility;

#endregion

namespace Framework.Core
{
    /// <summary>
    /// 事件命令
    /// </summary>
    public class SimpleCommand : ICommand
    {
        /// <summary>
        /// 命令执行方法
        /// </summary>
        /// <param name="message"></param>
        public virtual void Execute(INotification message)
        {
        }
    }

    /// <summary>
    /// 外观管理类
    /// </summary>
    public class Facade : Singleton<GameFacade>
    {
        #region Feilds And Properties

        private IController _controller;

        #endregion

        #region Constructor

        public Facade()
        {
            // ReSharper disable once VirtualMemberCallInContructor
            InitFramework();
        }

        #endregion

        #region Methods

        #region Framework Methods

        /// <summary>
        /// 初始化框架，命令管理器
        /// </summary>
        protected virtual void InitFramework()
        {
            if (_controller != null) return;
            _controller = Controller.Instance;
        }

        /// <summary>
        /// 注册消息到指定的命令
        /// </summary>
        /// <param name="commandName">消息名称</param>
        /// <param name="commandType">命令Type类型</param>
        public void RegisterCommand(string commandName, Type commandType)
        {
            _controller.RegisterCommand(commandName, commandType);
        }

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="commandName">消息名称</param>
        public void RemoveCommand(string commandName)
        {
            _controller.RemoveCommand(commandName);
        }

        /// <summary>
        /// 是否含有消息
        /// </summary>
        /// <param name="commandName">消息名称</param>
        /// <returns></returns>
        public bool HasCommand(string commandName)
        {
            return _controller.HasCommand(commandName);
        }

        /// <summary>
        /// 注册多个消息到指定命令
        /// </summary>
        /// <param name="commandType">命令Type类型</param>
        /// <param name="commandNames">消息名称</param>
        public void RegisterMultiCommand(Type commandType, params string[] commandNames)
        {
            var count = commandNames.Length;
            for (var node = 0; node < count; node++)
            {
                RegisterCommand(commandNames[node], commandType);
            }
        }

        /// <summary>
        /// 移除多个消息
        /// </summary>
        /// <param name="commandName"></param>
        public void RemoveMultiCommand(params string[] commandName)
        {
            var count = commandName.Length;
            for (var node = 0; node < count; node++)
            {
                RemoveCommand(commandName[node]);
            }
        }

        /// <summary>
        /// 发送消息体并执行
        /// </summary>
        /// <param name="notifyName">消息名称</param>
        /// <param name="body">消息内容</param>
        public void SendNotification(string notifyName, object body = null)
        {
            _controller.ExecuteCommand(new Notification(notifyName, body));
        }

        #endregion

        #endregion
    }
}