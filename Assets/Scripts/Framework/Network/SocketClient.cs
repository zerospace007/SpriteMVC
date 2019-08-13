#region
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Framework;
using Framework.Network;
using Framework.Utility;
using UnityEngine;
#endregion

public enum DisType
{
    Exception,
    Disconnect
}

public class SocketClient
{
    /// <summary>
    /// 最大读取长度
    /// </summary>
    private const int Max_Read = 8192;

    /// <summary>
    /// 最大读取二进制流
    /// </summary>
    private readonly byte[] m_ByteBuffer = new byte[Max_Read];

    /// <summary>
    /// Tcp客户端
    /// </summary>
    private TcpClient m_client;

    /// <summary>
    /// 读取erjinzhil
    /// </summary>
    private MemoryStream m_memStream;

    /// <summary>
    /// 发送二进制流
    /// </summary>
    private NetworkStream m_outStream;

    /// <summary>
    /// 二进制流读取
    /// </summary>
    private BinaryReader m_BinaryReader;

    /// <summary>
    /// 注册代理
    /// </summary>
    public void OnRegister()
    {
        m_memStream = new MemoryStream();
        m_BinaryReader = new BinaryReader(m_memStream);
    }

    /// <summary>
    /// 移除
    /// </summary>
    public void OnRemove()
    {
        Close();
        if (null != m_BinaryReader)
        {
            m_BinaryReader.Close();
            m_BinaryReader = null;
        }
       
        if (null != m_memStream)
        {
            m_memStream.Close();
            m_memStream = null;
        }
    }

    /// <summary>
    /// 连接服务器
    /// </summary>
    private void ConnectServer(string host, int port)
    {
        m_client = null;
        try
        {
            IPAddress[] address = Dns.GetHostAddresses(host);
            if (address.Length == 0)
            {
                Util.LogError("Network socket host invalid");
                return;
            }
            var addressFamily = address[0].AddressFamily == AddressFamily.InterNetworkV6 ?
                AddressFamily.InterNetworkV6 : AddressFamily.InterNetwork;
            m_client = new TcpClient(addressFamily);
            m_client.SendTimeout = 1000;
            m_client.ReceiveTimeout = 1000;
            m_client.NoDelay = true;
            m_client.BeginConnect(host, port, new AsyncCallback(OnConnect), null);
        }
        catch (Exception exception)
        {
            Close(); 
			Debug.LogError(exception.Message);
        }
    }

    /// <summary>
    /// 连接上服务器
    /// </summary>
    private void OnConnect(IAsyncResult asr)
    {
        m_outStream = m_client.GetStream();
        m_outStream.BeginRead(m_ByteBuffer, 0, Max_Read, new AsyncCallback(OnRead), null);
        NetworkManager.AddEvent(Protocal.Connect, string.Empty);
    }

    /// <summary>
    /// 写数据
    /// </summary>
    private void WriteMessage(byte[] message)
    {
        using (var memoryStream = new MemoryStream())
        {
            memoryStream.Position = 0;
            var binaryWriter = new BinaryWriter(memoryStream);
            var msglen = message.Length;
            binaryWriter.Write(msglen);
            binaryWriter.Write(message);
            binaryWriter.Flush();
            if (m_client != null && m_client.Connected)
            {
                var payload = memoryStream.ToArray();
                m_outStream.BeginWrite(payload, 0, payload.Length, OnWrite, null);
            }
            else
            {
                Debug.LogError("client.connected----->>false");
            }
        }
    }

    /// <summary>
    /// 读取消息
    /// </summary>
    private void OnRead(IAsyncResult asr)
    {
        try
        {
            int bytesRead;
            lock (m_client.GetStream())
            {
                //读取字节流到缓冲区
                bytesRead = m_client.GetStream().EndRead(asr);
            }
            if (bytesRead < 1)
            {
                //包尺寸有问题，断线处理
                OnDisconnected(DisType.Disconnect, "bytesRead < 1");
                return;
            }
            OnReceive(m_ByteBuffer, bytesRead); //分析数据包内容，抛给逻辑层
            lock (m_client.GetStream())
            {
                //分析完，再次监听服务器发过来的新消息
                Array.Clear(m_ByteBuffer, 0, m_ByteBuffer.Length); //清空数组
                m_client.GetStream().BeginRead(m_ByteBuffer, 0, Max_Read, OnRead, null);
            }
        }
        catch (Exception ex)
        {
            //PrintBytes();
            OnDisconnected(DisType.Exception, ex.Message);
        }
    }

    /// <summary>
    /// 丢失链接
    /// </summary>
    private void OnDisconnected(DisType dis, string msg)
    {
        Close(); //关掉客户端链接
        var protocal = dis == DisType.Exception ? Protocal.Exception : Protocal.Disconnect;

        var buffer = new ByteBuffer();
        buffer.WriteInt(protocal);
        NetworkManager.AddEvent(protocal, string.Empty);
        Util.LogError("Connection was closed by the server:>" + msg + " Distype:>" + dis);
    }

    /// <summary>
    /// 打印字节
    /// </summary>
    private void PrintBytes()
    {
        var returnStr = string.Empty;
        foreach (var tbyte in m_ByteBuffer)
        {
            returnStr += tbyte.ToString("X2");
        }
        Debug.LogWarning(returnStr);
    }

    /// <summary>
    /// 向链接写入数据流
    /// </summary>
    private void OnWrite(IAsyncResult result)
    {
        try
        {
            m_outStream.EndWrite(result);
        }
        catch (Exception ex)
        {
            Debug.LogError("OnWrite--->>>" + ex.Message);
        }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    private void OnReceive(byte[] bytes, int length)
    {
        m_memStream.Seek(0, SeekOrigin.End);
        m_memStream.Write(bytes, 0, length);
 
        m_memStream.Seek(0, SeekOrigin.Begin);
        while (RemainingBytes > 2)
        {
            var messageLen = m_BinaryReader.ReadInt32();
            if (RemainingBytes >= messageLen)
            {
                var memoryStream = new MemoryStream();
                var binaryWriter = new BinaryWriter(memoryStream);
                binaryWriter.Write(m_BinaryReader.ReadBytes(messageLen));
                memoryStream.Seek(0, SeekOrigin.Begin);
                OnReceivedMessage(memoryStream);
            }
            else
            {
                //Back up the position two bytes
                m_memStream.Position = m_memStream.Position - 4;
                break;
            }
        }
        //Create a new stream with any leftover bytes
        var leftover = m_BinaryReader.ReadBytes((int) RemainingBytes);
        m_memStream.SetLength(0); //Clear
        m_memStream.Write(leftover, 0, leftover.Length);
    }

    /// <summary>
    /// 剩余的字节
    /// </summary>
    private long RemainingBytes
    {
        get { return m_memStream.Length - m_memStream.Position; }
    }

    /// <summary>
    /// 接收到消息
    /// </summary>
    /// <param name="memoryStream"></param>
    private void OnReceivedMessage(MemoryStream memoryStream)
    {
        var binaryReader = new BinaryReader(memoryStream);
        var message = binaryReader.ReadBytes((int) (memoryStream.Length - memoryStream.Position));

        var buffer = Encoding.UTF8.GetString(message);
        NetworkManager.AddEvent(Protocal.Message, buffer);
    }


    /// <summary>
    /// 会话发送
    /// </summary>
    private void SessionSend(byte[] bytes)
    {
        WriteMessage(bytes);
    }

    /// <summary>
    /// 发送消息（此游戏通过Json协议，不再通过buffer封装）
    /// </summary>
    public void SendMessage(ByteBuffer buffer)
    {
        SessionSend(buffer.ToBytes());
        buffer.Close();
    }

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="data"></param>
    public void SendMessage(string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        SessionSend(bytes);
    }

    /// <summary>
    /// 关闭链接
    /// </summary>
    public void Close()
    {
        if (m_client != null)
        {
            if (m_client.Connected) m_client.Close();
            m_client = null;
        }
    }

    /// <summary>
    /// 发送连接服务器请求
    /// </summary>
    /// <param name="socketAddress">服务器IP</param>
    /// <param name="socketPort">服务器端口</param>
    public void SendConnect(string socketAddress, int socketPort)
    {
        ConnectServer(socketAddress, socketPort);
    }

    public bool IsConnected()
    {
        if (m_client == null)
            return false;

        return m_client.Connected;
    }
}