using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    /// <summary>
    /// 对象池管理器，分普通类对象池+资源游戏对象池
    /// </summary>
    public class PoolManager : MonoBehaviour
    {
        private Dictionary<string, object> m_ObjectPools = new Dictionary<string, object>();
        private Dictionary<string, GameObjectPool> m_GameObjectPools = new Dictionary<string, GameObjectPool>();

        /// <summary>
        /// 创建一个对象池
        /// </summary>
        /// <param name="poolName">对象池名称</param>
        /// <param name="prefab">创建对象</param>
        /// <param name="initSize">初始化大小</param>
        /// <returns></returns>
        public GameObjectPool CreatePool(string poolName, GameObject prefab, int initSize)
        {
            GameObjectPool objectPool = null;
            if (m_GameObjectPools.ContainsKey(poolName))
            {
                objectPool = m_GameObjectPools[poolName];
            }
            else
            {
                objectPool = new GameObjectPool(poolName, prefab, initSize);
                m_GameObjectPools[poolName] = objectPool;
            }
            return objectPool;
        }

        /// <summary>
        /// 根据对象池名称获取一个对象池
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public GameObjectPool GetPool(string poolName)
        {
            if (m_GameObjectPools.ContainsKey(poolName))
            {
                return m_GameObjectPools[poolName];
            }
            return null;
        }

        /// <summary>
        /// 根据对象池名称获取
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>
        public GameObject GetGameObject(string poolName)
        {
            GameObject result = null;
            if (m_GameObjectPools.ContainsKey(poolName))
            {
                GameObjectPool pool = m_GameObjectPools[poolName];
                result = pool.GetGameObject();
            }
            else
            {
                Debug.LogError("Invalid pool name specified: " + poolName);
            }
            return result;
        }

        /// <summary>
        /// 根据对象池名称回收游戏对象
        /// </summary>
        /// <param name="poolName"></param>
        /// <param name="go"></param>
        public void Recyle(string poolName, GameObject go)
        {
            if (m_GameObjectPools.ContainsKey(poolName))
            {
                GameObjectPool pool = m_GameObjectPools[poolName];
                pool.RecyleToPool(go);
            }
            else
            {
                Debug.LogWarning("No pool available with name: " + poolName);
            }
        }


        /// <summary>
        /// 创建泛型对象池
        /// </summary>
        /// <typeparam name="T">泛型class</typeparam>
        /// <param name="actionOnGet">获取委托</param>
        /// <param name="actionOnRecyle">回收委托</param>
        /// <returns></returns>
        public ObjectPool<T> CreatePool<T>(UnityAction<T> actionOnGet, UnityAction<T> actionOnRecyle) where T : class
        {
            var type = typeof(T);
            var pool = new ObjectPool<T>(actionOnGet, actionOnRecyle);
            m_ObjectPools[type.Name] = pool;
            return pool;
        }

        /// <summary>
        /// 根据泛型获取对象池
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ObjectPool<T> GetPool<T>() where T : class
        {
            var type = typeof(T);
            ObjectPool<T> pool = null;
            if (m_ObjectPools.ContainsKey(type.Name))
            {
                pool = m_ObjectPools[type.Name] as ObjectPool<T>;
            }
            return pool;
        }

        /// <summary>
        /// 根据泛型获取对象class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : class
        {
            var pool = GetPool<T>();
            if (pool != null)
            {
                return pool.Get();
            }
            return default;
        }

        /// <summary>
        /// 根据泛型回收对象class
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public void Recyle<T>(T obj) where T : class
        {
            var pool = GetPool<T>();
            if (pool != null)
            {
                pool.Recyle(obj);
            }
        }
    }
}