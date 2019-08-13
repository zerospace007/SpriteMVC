#region
using System;
#endregion

namespace Framework.Interfaces
{
    /// <summary>
    /// 命令控制接口
    /// </summary>
    public interface IController
    {
        /// <summary>
        /// 注册消息命令
        /// </summary>
        /// <param name="messageName">消息名称</param>
        /// <param name="commandType">消息命令执行类类型</param>
        void RegisterCommand(string messageName, Type commandType);

        /// <summary>
        /// 注册View消息命令
        /// </summary>
        /// <param name="view">IView接口</param>
        /// <param name="commandNames">消息参数数组</param>
        void RegisterViewCommand(IView view, string[] commandNames);

        /// <summary>
        /// 消息执行
        /// </summary>
        /// <param name="message">消息名称</param>
        void ExecuteCommand(INotification message);

        /// <summary>
        /// 移除消息
        /// </summary>
        /// <param name="messageName"></param>
        void RemoveCommand(string messageName);

        /// <summary>
        /// 移除View消息命令
        /// </summary>
        /// <param name="view">IView接口</param>
        /// <param name="commandNames">消息参数数组</param>
        void RemoveViewCommand(IView view, string[] commandNames);

        /// <summary>
        /// 判断是否含有消息
        /// </summary>
        /// <param name="messageName">消息名称</param>
        /// <returns></returns>
        bool HasCommand(string messageName);
    }
}