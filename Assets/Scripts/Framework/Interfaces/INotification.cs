namespace Framework.Interfaces
{
    /// <summary>
    /// 消息接口
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// 消息名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 消息内容
        /// </summary>
        object Body { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        string Type { get; set; }

        string ToString();
    }
}