#region
using System;
using System.Collections.Generic;
using Framework.Core;
using Framework.Interfaces;
using Framework.Utility;
using LitJson;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
#endregion

/// <summary>
/// 解包与下载界面显示
/// @Author NormanYang
/// </summary>
public class DownloadView : View
{
    #region Feilds and Properties
    public static string Name = "DownloadView";                     //界面名称
    private const string DownloadDesc = "DownloadDesc";             //进度描述配置文件名称
    private bool _isUnpackOrDownload;                               //true为下载，false为解包
    private string _currentShowStr;                                 //当前描述内容
    private int _unpackTotolCount;                                  //当前解包总数
    private int _unpackCurrentCount;                                //当前解包数量
    private TextAsset _downloadText;                                //游戏进度描述（TextAsset）
    private DownloadAndUnpackDesc _downloadAndUnpackDesc;           //游戏进度描述

    private Queue<object> _downloadingNotifyQueue;                  //已经下载文件通知队列
    private Queue<object> _progressNotifyQueue;                     //更新下载进度条队列

    private float _currentDownloadBytes;                            //当前已经下载字节数
    private float _totalDownloadBytes;                              //总共需要下载字节数

    private Slider _slider;                                         //进度条
    private Text _txtDesc;                                          //解包或者下载描述
    #endregion

    #region Methods

    private void Awake()
    {
        MessageArray = new string[]
        {
            NotifyName.UnpackOrDownload,
            NotifyName.UnpackTotolCount,
            NotifyName.UnpackUpdate,
            NotifyName.DownloadTotolBytes,
            NotifyName.DownloadUpdate,
            NotifyName.DownloadSpeed,
        };

        RemoveMessage(this, MessageArray);
        RegisterMessage(this, MessageArray);

        if (null != _downloadAndUnpackDesc) return;
        _downloadText = Resources.Load<TextAsset>(DownloadDesc);
        _downloadAndUnpackDesc = JsonMapper.ToObject<DownloadAndUnpackDesc>(_downloadText.text);

        _downloadingNotifyQueue = new Queue<object>();
        _progressNotifyQueue = new Queue<object>();

        _slider = transform.Find("Slider").GetComponent<Slider>();
        _txtDesc = transform.Find("TxtDesc").GetComponent<Text>();
    }

    /// <summary>
    /// 处理View消息
    /// </summary>
    /// <param name="message"></param>
    public override void OnMessage(INotification message)
    {
        var msgName = message.Name;
        var body = message.Body;
        switch (msgName)
        {
            case NotifyName.UnpackOrDownload:       //是解压还是下载
                UpdateDescContent(body);
                break;
            case NotifyName.UnpackTotolCount:       //总共需要解压的个数
                _unpackTotolCount = Convert.ToInt32(body);
                break;
            case NotifyName.UnpackUpdate:           //解压一个完成
                _unpackCurrentCount = Convert.ToInt32(body);
                UnpackUpdate();
                break;

            case NotifyName.DownloadTotolBytes:     //需要下载的总大小
                _totalDownloadBytes = Convert.ToSingle(body);
                DownloadTotolBytes();
                break;

            case NotifyName.DownloadUpdate:         //更新下载完成一个资源
                AddDownloadEvent(body);
                break;
            case NotifyName.DownloadSpeed:          //更新下载速度
                AddProgressEvent(body);
                break;
        }
    }

    private void OnEnable()
    {
        SpriteAtlasManager.atlasRequested += SetAtlas;
    }

    private void OnDisable()
    {
        SpriteAtlasManager.atlasRequested -= SetAtlas;
    }

    /// <summary>
    /// 赋值Atlas
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="action"></param>
    private void SetAtlas(string tag, Action<SpriteAtlas> callback)
    {
        SpriteAtlas spriteAtlas = Resources.Load<SpriteAtlas>("Atlas/" + tag);
        callback(spriteAtlas);
    }

    /// <summary>
    /// 更新游戏解包或者下载进度描述
    /// </summary>
    /// <param name="body"></param>
    private void UpdateDescContent(object body)
    {
        if (null == _downloadAndUnpackDesc) return;

        _isUnpackOrDownload = Convert.ToBoolean(body);
        _currentShowStr = _isUnpackOrDownload ? _downloadAndUnpackDesc.DownloadDesc : _downloadAndUnpackDesc.UnpackDesc;
        _txtDesc.text = _currentShowStr + "...";
    }

    /// <summary>
    /// 更新游戏解包进度条
    /// </summary>
    private void UnpackUpdate()
    {
        float unpackProgress = (float)_unpackCurrentCount / _unpackTotolCount;
        Util.Log("解包进度" + unpackProgress);

        _slider.value = unpackProgress;
    }

    private void DownloadTotolBytes()
    {
        if (_totalDownloadBytes == 0) SendNotification(NotifyName.ResourceInited);
    }

    /// <summary>
    /// 更新下载进度条
    /// </summary>
    /// <param name="data"></param>
    private void DownloadUpdate(object data)
    {
        _currentDownloadBytes += Convert.ToSingle(data);

        float downProgress = _currentDownloadBytes / _totalDownloadBytes;
        Util.Log("下载进度" + downProgress);

        _slider.value = downProgress;

        if (downProgress == 1) SendNotification(NotifyName.ResourceInited);
    }

    /// <summary>
    /// 更新下载速度
    /// </summary>
    /// <param name="data"></param>
    private void DownloadSpeed(object data)
    {
        if (null == data) return;
        var progressSpeed = data.ToString();
        var downSpeed = progressSpeed + "...";
        _txtDesc.text = _currentShowStr + downSpeed;
        Util.Log("下载速度" + downSpeed);
    }

    /// <summary>
    /// 添加下载速度通知队列
    /// </summary>
    /// <param name="obj"></param>
    private void AddProgressEvent(object obj)
    {
        _progressNotifyQueue.Enqueue(obj);
    }

    /// <summary>
    /// 添加下载文件完成通知队列
    /// </summary>
    /// <param name="obj"></param>
    private void AddDownloadEvent(object obj)
    {
        _downloadingNotifyQueue.Enqueue(obj);
    }

    /// <summary>
    /// 获取下载速度队列，更新显示
    /// </summary>
    private void Update()
    {
        if (_downloadingNotifyQueue.Count > 0)
        {
            while (_downloadingNotifyQueue.Count > 0)
            {
                var obj = _downloadingNotifyQueue.Dequeue();
                DownloadUpdate(obj);
            }
        }

        if (_progressNotifyQueue.Count <= 0) return;
        while (_progressNotifyQueue.Count > 0)
        {
            var obj = _progressNotifyQueue.Dequeue();
            DownloadSpeed(obj);
        }
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    private void OnDestroy()
    {
        if (null != _downloadText)
        {
            Resources.UnloadAsset(_downloadText);
            _downloadText = null;
        }
        _downloadAndUnpackDesc = null;
    }

    private class DownloadAndUnpackDesc
    {
        public string DownloadDesc { get; set; }
        public string UnpackDesc { get; set; }
    }
    #endregion
}