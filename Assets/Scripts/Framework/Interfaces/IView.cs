namespace Framework.Interfaces
{
    /// <summary>
    /// View接口
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="message"></param>
        void OnMessage(INotification message);
    }
}