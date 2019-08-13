#region

using Framework.Interfaces;

#endregion

namespace Framework.Core
{
    /// <summary>
    /// 消息体类
    /// </summary>
    public class Notification : INotification
    {
        /// <summary>
        /// Get the string representation of the <c>Notification instance</c>
        /// </summary>
        /// <returns>The string representation of the <c>Notification</c> instance</returns>
        public override string ToString()
        {
            var msg = "Notification Name: " + Name;
            msg += "\nBody:" + (Body == null ? "null" : Body.ToString());
            msg += "\nType:" + (Type ?? "null");
            return msg;
        }

        #region Constructors

        /// <summary>
        /// 构造消息OverLoad
        /// </summary>
        /// <param name="name">消息名称</param>
        public Notification(string name) : this(name, null, null)
        {
        }

        /// <summary>
        /// 构造消息OverLoad
        /// </summary>
        /// <param name="name">消息名称</param>
        /// <param name="body">消息参数内容</param>
        public Notification(string name, object body) : this(name, body, null)
        {
        }

        /// <summary>
        /// 构造消息
        /// </summary>
        /// <param name="name">消息名称</param>
        /// <param name="body">消息参数内容</param>
        /// <param name="type">消息类型</param>
        public Notification(string name, object body, string type)
        {
            m_Name = name;
            m_Body = body;
            m_Type = type;
        }

        #endregion

        #region Feilds And Properties

        /// <summary>
        /// The name of the <c>Notification</c> instance
        /// </summary>
        public virtual string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// The body of the <c>Notification</c> instance
        /// </summary>
        /// <remarks>This accessor is thread safe</remarks>
        public virtual object Body
        {
            get
            {
                // Setting and getting of reference types is atomic, no need to lock here
                return m_Body;
            }
            set
            {
                // Setting and getting of reference types is atomic, no need to lock here
                m_Body = value;
            }
        }

        /// <summary>
        /// The type of the <c>Notification</c> instance
        /// </summary>
        /// <remarks>This accessor is thread safe</remarks>
        public virtual string Type
        {
            get
            {
                // Setting and getting of reference types is atomic, no need to lock here
                return m_Type;
            }
            set
            {
                // Setting and getting of reference types is atomic, no need to lock here
                m_Type = value;
            }
        }

        /// <summary>
        /// The name of the notification instance 
        /// </summary>
        private readonly string m_Name;

        /// <summary>
        /// The type of the notification instance
        /// </summary>
        private string m_Type;

        /// <summary>
        /// The body of the notification instance
        /// </summary>
        private object m_Body;

        #endregion
    }
}