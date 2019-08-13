namespace Framework.Network
{
    public class Protocal
    {
        public const int Connect = 100101;             //连接服务器
        public const int Exception = 100102;           //异常掉线
        public const int Disconnect = 100103;          //正常断线   
        public const int Message = 100104;             //接收消息
    }
}