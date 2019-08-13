#region
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
#endregion

namespace Framework.Utility
{

    #region Generic Singleton

    /// <summary>
    /// 泛型单例
    /// </summary>
    /// <typeparam name="T">泛型单例</typeparam>
    public class Singleton<T>
    {
        #region Properties

        /// <summary>
        /// 静态单例对象
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                object lockObj;
                Monitor.Enter(lockObj = LockObj);
                try
                {
                    if (_instance == null)
                    {
                        _instance = (T)Activator.CreateInstance(typeof(T), true);
                    }
                }
                finally
                {
                    Monitor.Exit(lockObj);
                }
                return _instance;
            }
            set { _instance = value; }
        }

        #endregion

        #region Feilds

        private static T _instance; //静态单例对象
        private static readonly object LockObj = new object(); //线程锁定对象

        #endregion
    }

    #endregion

    #region Monobehaviour Singleton

    /// <summary>
    /// 组件式单例
    /// Author NormanYang
    /// </summary>
    public class Singleton : MonoBehaviour
    {
        #region Feilds And Properties

        /// <summary>
        /// 默认管理器对象名称
        /// </summary>
        public const string GameManagerName = "GameManager";

        /// <summary>
        /// 管理器挂载的游戏对象
        /// </summary>
        private static GameObject m_ManagerObj;

        /// <summary>
        /// 组件词典，存储管理器组件
        /// </summary>
        private static readonly IDictionary<string, Component> mSingletonMap = new Dictionary<string, Component>();

        #endregion

        #region Game Manager Methods

        /// <summary>
        /// 获取组件挂载的游戏对象
        /// </summary>
        /// <param name="objName">游戏对象名称</param>
        private static GameObject GetManagerObj(string objName)
        {
            var gameObj = GameObject.Find(objName);
            if (null != gameObj) return gameObj;
            gameObj = new GameObject(objName);
            return gameObj;
        }

        /// <summary>
        /// 添加获取管理器
        /// 默认为三种情况，一种无参数，另两种有一个参数
        /// 没有传递参数时，默认挂在GameManager上
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <returns></returns>
        public static T GetInstance<T>() where T : Component
        {
            return GetSingleInstance<T>(GameManagerName);
        }

        /// <summary>
        /// 添加获取管理器
        /// 默认为三种情况，一种无参数，另两种有一个参数
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="objName">游戏对象</param>
        /// <returns></returns>
        public static T GetInstance<T>(string objName) where T : Component
        {
            return GetSingleInstance<T>(objName);
        }

        /// <summary>
        /// 添加获取管理器
        /// 默认为三种情况，一种无参数，另两种有一个参数
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="gameObj">游戏对象</param>
        /// <returns></returns>
        public static T GetInstance<T>(GameObject gameObj) where T : Component
        {
            return GetSingleInstance<T>(string.Empty, gameObj);
        }

        /// <summary>
        /// 添加获取管理器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objName">游戏对象名称，默认为空字符串</param>
        /// <param name="gameObj">游戏对象，默认为空值</param>
        /// <returns></returns>
        private static T GetSingleInstance<T>(string objName = "", GameObject gameObj = null) where T : Component
        {
            var instanceName = typeof(T).Name;
            if (mSingletonMap.ContainsKey(instanceName)) return (T)mSingletonMap[instanceName];
            //首次赋值并加入Map管理
            m_ManagerObj = gameObj == null ? GetManagerObj(objName) : gameObj;
            DontDestroyOnLoad(m_ManagerObj);

            mSingletonMap.Add(instanceName, m_ManagerObj.AddComponent<T>());
            return (T)mSingletonMap[instanceName];
        }
        #endregion
    }

    #endregion
}