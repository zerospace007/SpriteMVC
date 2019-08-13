using System;
using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Framework
{
    /// <summary>
    /// GameObject对象池
    /// </summary>
    public class GameObjectPool
    {
        /// <summary>
        /// 对象池根Transform
        /// </summary>
        private Transform m_PoolRoot;

        /// <summary>
        /// 初始化对象池大小
        /// </summary>
        private readonly int m_InitSize;
        /// <summary>
        /// 对象池名称
        /// </summary>
        private readonly string m_PoolName;
       
        /// <summary>
        /// 创建对象
        /// </summary>
        private readonly GameObject prefab;
        /// <summary>
        /// 对象栈
        /// </summary>
        private Stack<GameObject> m_PoolStack = new Stack<GameObject>();

        /// <summary>
        /// 回收执行
        /// </summary>
        public Action<GameObject> RecyleHandler;

        /// <summary>
        /// 对象池跟对象
        /// </summary>
        public Transform PoolRoot
        {
            get
            {
                if (m_PoolRoot == null)
                {
                    var objectPool = new GameObject("Pool@" + m_PoolName);
                    //UObject.DontDestroyOnLoad(objectPool);
                    objectPool.transform.localScale = Vector3.one;
                    objectPool.transform.localPosition = Vector3.zero;
                    m_PoolRoot = objectPool.transform;
                }
                return m_PoolRoot;
            }
        }

        /// <summary>
        /// 构造游戏对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="prefab">创建对象</param>
        /// <param name="initCount">初始化数量</param>
        /// <param name="poolRoot">对象池根</param>
        public GameObjectPool(string poolName, GameObject prefab, int initCount)
        {
            m_PoolName = poolName;
            m_InitSize = initCount;
            this.prefab = prefab;

            //初始化对象池
            for (int index = 0; index < initCount; index++)
            {
                GameObject gameObj = UObject.Instantiate(prefab) as GameObject;
                RecyleToPool(gameObj);
            }
        }

        /// <summary>
        /// 获取游戏对象
        /// </summary>
        /// <returns></returns>
        public GameObject GetGameObject()
        {
            GameObject gameObj;
            if (m_PoolStack.Count > 0)
            {
                gameObj = m_PoolStack.Pop();
            }
            else
            {
                gameObj = UObject.Instantiate(prefab) as GameObject;
                gameObj.transform.SetParent(PoolRoot, false);
            }
            gameObj.SetActive(true);
            return gameObj;
        }

        /// <summary>
        /// 回收
        /// </summary>
        /// <param name="poName"></param>
        /// <param name="gameObj"></param>
        public void RecyleToPool(GameObject gameObj)
        {
            if (gameObj == null) return;
            RecyleHandler?.Invoke(gameObj);

            gameObj.SetActive(false);
            m_PoolStack.Push(gameObj);
            gameObj.transform.SetParent(PoolRoot, false);
        }
    }
}
