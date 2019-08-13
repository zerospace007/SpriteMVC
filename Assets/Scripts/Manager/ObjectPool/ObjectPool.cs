using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    /// <summary>
    /// 对象池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPool<T> where T : class
    {
        /// <summary>
        /// 对象栈
        /// </summary>
        private readonly Stack<T> m_Stack = new Stack<T>();
        private readonly UnityAction<T> m_ActionOnGet;
        private readonly UnityAction<T> m_ActionOnRelease;

        public int countAll { get; private set; }
        public int countActive { get { return countAll - countInactive; } }
        public int countInactive { get { return m_Stack.Count; } }

        /// <summary>
        /// 创建对象池
        /// </summary>
        /// <param name="actionOnGet"></param>
        /// <param name="actionOnRelease"></param>
        public ObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
        {
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T element = m_Stack.Pop();
            if(element == null) element = (T)Activator.CreateInstance(typeof(T), true);
            m_ActionOnGet?.Invoke(element);
            return element;
        }

        /// <summary>
        /// 回事对象
        /// </summary>
        /// <param name="element"></param>
        public void Recyle(T element)
        {
            if (m_Stack.Count > 0 && ReferenceEquals(m_Stack.Peek(), element))
                Debug.LogError("Internal error. Trying to destroy object that is already recyle to pool.");
            m_ActionOnRelease?.Invoke(element);
            m_Stack.Push(element);
        }
    }
}
