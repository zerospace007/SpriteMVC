namespace Framework.Interfaces
{
    /// <summary>
    /// 命令执行接口
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// 执行命令方法
        /// </summary>
        /// <param name="message">IMessage消息接口</param>
        void Execute(INotification message);
    }
}