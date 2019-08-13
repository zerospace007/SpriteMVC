#region 命名空间
using System.Collections.Generic;
using Framework.Utility;
using LuaInterface;
using UnityEngine;
using UnityEngine.UI;
using UObject = UnityEngine.Object;
#endregion

namespace Framework
{
    /// <summary>
    /// 界面管理类
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region gui or map parent
        private Transform m_NormalCanvas;                   //全屏UI或者弹出UI根对象
        private Transform m_MapCanvas;                      //Map UI 跟对象
        private Transform m_GuideCanvas;                    //引导UI根对象
        private Material m_GreyMaterial;                    //灰度材质

        /// <summary>
        /// 灰色材质
        /// </summary>
        public Material Grey
        {
            get
            {
                if (null == m_GreyMaterial) m_GreyMaterial = Resources.Load<Material>("Material/Grey");
                return m_GreyMaterial;
            }
        }

        /// <summary>
        /// gui根对象
        /// </summary>
        public Transform NormalCanvas
        {
            get
            {
                if (m_NormalCanvas == null)
                {
                    GameObject gameObj = GameObject.FindWithTag("NormalCanvas");
                    if (gameObj != null) m_NormalCanvas = gameObj.transform;
                }
                return m_NormalCanvas;
            }
        }

        /// <summary>
        /// Map根对象
        /// </summary>
        public Transform MapCanvas
        {
            get
            {
                if (m_MapCanvas == null)
                {
                    GameObject gameObj = GameObject.FindWithTag("MapCanvas");
                    if (gameObj != null) m_MapCanvas = gameObj.transform;
                }
                return m_MapCanvas;
            }
        }

        /// <summary>
        /// 引导根对象
        /// </summary>
        public Transform GuideCanvas
        {
            get
            {
                if (null == m_GuideCanvas)
                {
                    GameObject gameObj = GameObject.FindWithTag("GuideCanvas");
                    if (gameObj != null) m_GuideCanvas = gameObj.transform;
                }
                return m_GuideCanvas;
            }
        }
        #endregion

        private IDictionary<string, GameObject> m_UIDict = new Dictionary<string, GameObject>();
        private Stack<GameObject> m_UIStack = new Stack<GameObject>();

        /// <summary>
        /// sprite置灰
        /// </summary>
        /// <param name="sprite"></param>
        /// <param name="isGrey"></param>
        public void SetGrey(Image sprite, bool isGrey)
        {
            if (isGrey) sprite.material = Grey;
            else sprite.material = null;
        }

        /// <summary>
        /// 推入一个UI界面
        /// </summary>
        /// <param name="nextContext"></param>
        public void Push(GameObject nextContext)
        {
            if (m_UIStack.Count != 0)
            {
                GameObject curContext = m_UIStack.Peek();
                curContext.SetActive(false);
            }

            m_UIStack.Push(nextContext);
            nextContext.SetActive(true);
        }

        /// <summary>
        /// 推出一个UI界面
        /// </summary>
        public void Pop()
        {
            if (m_UIStack.Count != 0)
            {
                GameObject curContext = m_UIStack.Peek();
                m_UIStack.Pop();
                curContext.SetActive(false);
            }

            if (m_UIStack.Count != 0)
            {
                GameObject lastContext = m_UIStack.Peek();
                lastContext.SetActive(true);
            }
        }

        /// <summary>
        /// 获取当前ui界面
        /// </summary>
        /// <returns></returns>
        public GameObject PeekOrNull()
        {
            if (m_UIStack.Count != 0)
            {
                return m_UIStack.Peek();
            }
            return null;
        }

        /// <summary>
        /// 创建显示界面
        /// </summary>
        /// <param name="uiName">ui界面名称</param>
        /// <param name="luaFunction">lua执行函数</param>
        /// <returns></returns>
        public void ShowUI(string uiName, LuaFunction luaFunction = null, LuaTable luaTable = null)
        {
            if (!m_UIDict.ContainsKey(uiName) || m_UIDict[uiName] == null)
            {
                string fullName = "ui/" + uiName.ToLower() + GameConst.BundleSuffix;
                var ResourceManager = Singleton.GetInstance<ResourceManager>();
                ResourceManager.LoadPrefab(fullName, uiName, OnLoadedUI);
                void OnLoadedUI(UObject[] objects)
                {
                    if (objects.Length == 0) return;
                    GameObject prefab = objects[0] as GameObject;
                    if (prefab == null) return;

                    GameObject uiObject = Instantiate(prefab) as GameObject;
                    uiObject.name = uiName;
                    var tranform = uiObject.transform;
                    Util.SetParent(NormalCanvas, tranform);
                    tranform.localScale = Vector3.one;
                    tranform.localPosition = Vector3.zero;
                    tranform.localRotation = Quaternion.identity;
                    uiObject.AddComponent<LuaBehaviour>();
                    m_UIDict[uiName] = uiObject;
                    if (luaFunction != null) luaFunction.Call(luaTable, uiObject);
                    Debug.Log("CreateUI::>> " + uiName);
                }
            }
            else
            {
                var uiObject = m_UIDict[uiName];
                uiObject.SetActive(true);
                if (luaFunction != null) luaFunction.Call(luaTable, uiObject);
                Debug.Log("ShowUI::>> " + uiName);
                if (uiName == "MainMenuView") return;
                uiObject.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// 创建显示界面
        /// </summary>
        /// <param name="uiName">ui界面名称</param>
        /// <param name="typeName">组件类型名称</param>
        public void ShowUI2(string uiName, string typeName = null)
        {
            if (!m_UIDict.ContainsKey(uiName) || m_UIDict[uiName] == null)
            {
                string fullName = "ui/" + uiName.ToLower();
                GameObject prefab = Resources.Load<GameObject>(fullName);
                if (prefab == null) return;

                GameObject uiObject = Instantiate(prefab) as GameObject;
                uiObject.name = uiName;
                var tranform = uiObject.transform;
                Util.SetParent(NormalCanvas, tranform);
                tranform.localScale = Vector3.one;
                tranform.localPosition = Vector3.zero;
                tranform.localRotation = Quaternion.identity;
                typeName = string.IsNullOrEmpty(typeName) ? uiName : typeName;
                Util.AddComponent(uiObject, typeName);
                m_UIDict[uiName] = uiObject;
                Debug.Log("CreateUI::>> " + uiName);
            }
            else
            {
                var uiObject = m_UIDict[uiName];
                uiObject.SetActive(true);
                Debug.Log("ShowUI::>> " + uiName);
            }
        }

        /// <summary>
        /// 隐藏界面
        /// </summary>
        /// <param name="uiName"></param>
        public void HideUI(string uiName)
        {
            if (!m_UIDict.ContainsKey(uiName)) return;

            var uiObject = m_UIDict[uiName];
            uiObject.SetActive(false);
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiName"></param>
        public void CloseUI(string uiName)
        {
            if (!m_UIDict.ContainsKey(uiName)) return;

            if (m_UIDict[uiName] != null)
            {
                Destroy(m_UIDict[uiName]);
                string abName = "ui/" + uiName.ToLower() + GameConst.BundleSuffix;
                var ResourceManager = Singleton.GetInstance<ResourceManager>();
                ResourceManager.UnloadAssetBundle(abName);
            }

            m_UIDict.Remove(uiName);
        }
    }
}