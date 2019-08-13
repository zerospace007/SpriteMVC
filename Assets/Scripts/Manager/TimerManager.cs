#region
using System.Collections.Generic;
using Framework.Core;
using UnityEngine;
#endregion

namespace Framework
{
    /// <summary>
    /// 时间戳对象
    /// </summary>
    public class TimerInfo
    {
        #region Constructor

        public TimerInfo(Object target)
        {
            Target = target;
            IsDelete = false;
        }

        #endregion

        #region Feilds

        public long Tick;
        public bool IsStop;
        public bool IsDelete;
        public Object Target;

        #endregion
    }

    /// <summary>
    /// 时间管理
    /// @Author NormanYang
    /// </summary>
    public class TimerManager : View
    {
        #region Properties

        public float Interval { get; set; }
        private readonly List<TimerInfo> m_TimerInfoList = new List<TimerInfo>();

        #endregion

        #region Mono Methods

        private void Start()
        {
            StartTimer(GameConst.TimerInterval);
        }

        #endregion

        #region Methods

        /// <summary>
        /// 启动计时器
        /// </summary>
        public void StartTimer(float value)
        {
            Interval = value;
            InvokeRepeating("Run", 0, Interval);
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void StopTimer()
        {
            CancelInvoke("Run");
        }

        /// <summary>
        /// 添加计时器事件
        /// </summary>
        public void AddTimerEvent(TimerInfo info)
        {
            if (!m_TimerInfoList.Contains(info))
            {
                m_TimerInfoList.Add(info);
            }
        }

        /// <summary>
        /// 删除计时器事件
        /// </summary>
        /// <param name="info"></param>
        public void RemoveTimerEvent(TimerInfo info)
        {
            if (m_TimerInfoList.Contains(info) && info != null)
            {
                info.IsDelete = true;
            }
        }

        /// <summary>
        /// 停止计时器事件
        /// </summary>
        /// <param name="info"></param>
        public void StopTimerEvent(TimerInfo info)
        {
            if (m_TimerInfoList.Contains(info) && info != null)
            {
                info.IsStop = true;
            }
        }

        /// <summary>
        /// 继续计时器事件
        /// </summary>
        /// <param name="info"></param>
        public void ResumeTimerEvent(TimerInfo info)
        {
            if (m_TimerInfoList.Contains(info) && info != null)
            {
                info.IsStop = false;
            }
        }

        /// <summary>
        /// 计时器运行
        /// </summary>
        private void Run()
        {
            if (m_TimerInfoList.Count == 0) return;
            for (int node = 0, count = m_TimerInfoList.Count; node < count; node++)
            {
                var timeInfo = m_TimerInfoList[node];
                if (timeInfo.IsDelete || timeInfo.IsStop)
                {
                    continue;
                }
                var timer = timeInfo.Target as ITimerBehaviour;
                timer.TimerUpdate();
                timeInfo.Tick++;
            }
            /////////////////////////清除标记为删除的事件///////////////////////////
            for (var redo = m_TimerInfoList.Count - 1; redo >= 0; redo--)
            {
                if (m_TimerInfoList[redo].IsDelete)
                {
                    m_TimerInfoList.Remove(m_TimerInfoList[redo]);
                }
            }
        }
        #endregion
    }
}