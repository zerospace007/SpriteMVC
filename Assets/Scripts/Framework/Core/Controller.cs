#region
using System;
using System.Collections.Generic;
using Framework.Interfaces;
using Framework.Utility;
#endregion

namespace Framework.Core
{
    /// <summary>
    /// 命令控制管理
    /// </summary>
    public sealed class Controller : Singleton<Controller>, IController
    {
        #region Constructor

        /// <summary>
        /// 构建命令控制管理，初始化存储词典
        /// </summary>
        private Controller()
        {
            InitializeController();
        }

        #endregion

        #region Feilds

        private IDictionary<string, Type> _commandMap;          //命令词典
        private IDictionary<IView, List<string>> _viewCmdMap;   //界面命令词典

        private readonly object _syncRoot = new object();       //锁定对象

        #endregion

        #region Methods

        /// <summary>
        /// 初始化命令对照词典
        /// </summary>
        private void InitializeController()
        {
            _commandMap = new Dictionary<string, Type>();
            _viewCmdMap = new Dictionary<IView, List<string>>();
        }

        /// <summary>
        /// 执行命令
        /// </summary>
        /// <param name="note"></param>
        public void ExecuteCommand(INotification note)
        {
            Type commandType = null;
            List<IView> views = null;
            lock (_syncRoot)
            {
                if (_commandMap.ContainsKey(note.Name))
                {
                    commandType = _commandMap[note.Name];
                }
                else
                {
                    views = new List<IView>();
                    foreach (var view in _viewCmdMap)
                    {
                        if (view.Value.Contains(note.Name))
                        {
                            views.Add(view.Key);
                        }
                    }
                }
            }
            if (commandType != null)
            {
                //Controller
                var commandInstance = Activator.CreateInstance(commandType);
                if (commandInstance is ICommand)
                {
                    ((ICommand) commandInstance).Execute(note);
                }
            }
            if (views == null || views.Count <= 0) return;
            for (int node = 0, count = views.Count; node < count; node++)
            {
                views[node].OnMessage(note);
            }
        }

        /// <summary>
        /// 注册通知
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="commandType"></param>
        public void RegisterCommand(string commandName, Type commandType)
        {
            lock (_syncRoot)
            {
                _commandMap[commandName] = commandType;
            }
        }

        /// <summary>
        /// 是否有命令
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public bool HasCommand(string commandName)
        {
            lock (_syncRoot)
            {
                return _commandMap.ContainsKey(commandName);
            }
        }

        /// <summary>
        /// 移除通知名称
        /// </summary>
        /// <param name="commandName"></param>
        public void RemoveCommand(string commandName)
        {
            lock (_syncRoot)
            {
                if (_commandMap.ContainsKey(commandName))
                {
                    _commandMap.Remove(commandName);
                }
            }
        }

        /// <summary>
        /// 注册某View通知列表
        /// </summary>
        /// <param name="view"></param>
        /// <param name="commandNames"></param>
        public void RegisterViewCommand(IView view, string[] commandNames)
        {
            lock (_syncRoot)
            {
                if (_viewCmdMap.ContainsKey(view))
                {
                    List<string> list;
                    if (!_viewCmdMap.TryGetValue(view, out list)) return;
                    for (int node = 0, count = commandNames.Length; node < count; node++)
                    {
                        if (list.Contains(commandNames[node])) continue;
                        list.Add(commandNames[node]);
                    }
                }
                else
                {
                    _viewCmdMap.Add(view, new List<string>(commandNames));
                }
            }
        }

        /// <summary>
        /// 移除某View通知列表
        /// </summary>
        /// <param name="view">某View</param>
        /// <param name="commandNames">通知列表</param>
        public void RemoveViewCommand(IView view, string[] commandNames)
        {
            lock (_syncRoot)
            {
                if (!_viewCmdMap.ContainsKey(view)) return;
                List<string> list;
                if (!_viewCmdMap.TryGetValue(view, out list)) return;
                for (int node = 0, count = commandNames.Length; node < count; node++)
                {
                    if (!list.Contains(commandNames[node])) continue;
                    list.Remove(commandNames[node]);
                }
            }
        }

        #endregion
    }
}