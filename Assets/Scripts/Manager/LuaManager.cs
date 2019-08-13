using Framework.Utility;
using LuaInterface;
using UnityEngine;

namespace Framework
{
    public partial class LuaManager : MonoBehaviour
    {
        public LuaState mLuaState;              //lua状态
        private LuaLoader m_LuaLoader;          //lua 加载管理
        private LuaLooper m_LuaLooper;          //looper组件

        /// <summary>
        /// 初始化赋值
        /// </summary>
        private void Awake()
        {
            m_LuaLoader = new LuaLoader();
            mLuaState = new LuaState();
            OpenLibs();
            mLuaState.LuaSetTop(0);

            LuaBinder.Bind(mLuaState);
            DelegateFactory.Init();
            LuaCoroutine.Register(mLuaState, this);
        }

        /// <summary>
        /// 初始化启动
        /// </summary>
        public void InitStart()
        {
            InitLuaPath();                                  //lua目录
            InitLuaBundle();                                //lua bundle目录
            mLuaState.Start();                              //启动LUAVM
            StartMain();                                    //主调用，真机不执行
            StartLooper();                                  //开启循环Update等
        }

        /// <summary>
        /// 添加looper组件
        /// </summary>
        private void StartLooper()
        {
            m_LuaLooper = gameObject.AddComponent<LuaLooper>();
            m_LuaLooper.mLuaState = mLuaState;
        }

        /// <summary>
        /// cjson 比较特殊，只new了一个table，没有注册库，这里注册一下
        /// </summary>
        protected void OpenCJson()
        {
            mLuaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            mLuaState.OpenLibs(LuaDLL.luaopen_cjson);
            mLuaState.LuaSetField(-2, "cjson");

            mLuaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            mLuaState.LuaSetField(-2, "cjson.safe");
        }

        /// <summary>
        /// 测试调试用途，真机不执行
        /// </summary>
        private void StartMain()
        {
            mLuaState.DoFile("Main.lua");

            LuaFunction mainFunc = mLuaState.GetFunction("Main");
            if (null == mainFunc) return;
            mainFunc.Call();
            mainFunc.Dispose();
            mainFunc = null;
        }

        /// <summary>
        /// 初始化加载第三方库
        /// </summary>
        private void OpenLibs()
        {
            //mLuaState.OpenLibs(LuaDLL.luaopen_pb);
            //mLuaState.OpenLibs(LuaDLL.luaopen_sproto_core);
            //mLuaState.OpenLibs(LuaDLL.luaopen_protobuf_c);
            //mLuaState.OpenLibs(LuaDLL.luaopen_lpeg);
            //mLuaState.OpenLibs(LuaDLL.luaopen_bit);
            //mLuaState.OpenLibs(LuaDLL.luaopen_socket_core);

            OpenCJson();
        }

        /// <summary>
        /// 初始化Lua代码加载路径
        /// </summary>
        private void InitLuaPath()
        {
            if (GameConst.DebugMode)
            {
                string rootPath = Application.dataPath;
                mLuaState.AddSearchPath(rootPath + "/Lua");
                mLuaState.AddSearchPath(rootPath + "/ToLua/Lua");
            }
            else
            {
                mLuaState.AddSearchPath(Util.DataPath + "lua");
            }
        }

        /// <summary>
        /// 初始化LuaBundle
        /// </summary>
        private void InitLuaBundle()
        {
            if (!m_LuaLoader.beZip) return;
            m_LuaLoader.AddBundle("lua/lua");
            m_LuaLoader.AddBundle("lua/lua_cjson");
            m_LuaLoader.AddBundle("lua/lpeg");
            m_LuaLoader.AddBundle("lua/lua_misc");
            m_LuaLoader.AddBundle("lua/lua_system");
            m_LuaLoader.AddBundle("lua/lua_system_Injection");
            m_LuaLoader.AddBundle("lua/lua_system_reflection");
            m_LuaLoader.AddBundle("lua/lua_unityengine");

            //m_LuaLoader.AddBundle("lua/lua_protobuf");
            //m_LuaLoader.AddBundle("lua/lua_3rd_cjson");
            //m_LuaLoader.AddBundle("lua/lua_3rd_luabitop");
            //m_LuaLoader.AddBundle("lua/lua_3rd_pbc");
            //m_LuaLoader.AddBundle("lua/lua_3rd_pblua");
            //m_LuaLoader.AddBundle("lua/lua_3rd_sproto");
        }

        /// <summary>
        /// 执行lua模块
        /// </summary>
        /// <param name="filename"></param>
        public void DoFile(string filename)
        {
            mLuaState.DoFile(filename);
        }

        /// <summary>
        /// 添加lua代码Bundle文件
        /// </summary>
        /// <param name="bundleName"></param>
        public void AddBundle(string bundleName)
        {
            m_LuaLoader.AddBundle("lua/" + bundleName);
        }

        /// <summary>
        /// 调用Lua GC 清理内存
        /// </summary>
        public void LuaGC()
        {
            if (null == mLuaState) return;
            mLuaState.LuaGC(LuaGCOptions.LUA_GCCOLLECT);
        }

        /// <summary>
        /// 关闭Lua调用
        /// </summary>
        public void Close()
        {
            if (m_LuaLooper != null)
            {
                m_LuaLooper.Destroy();
                m_LuaLooper = null;
            }

            if (mLuaState != null)
            {
                mLuaState.Dispose();
                mLuaState = null;
            }

            m_LuaLoader = null;
        }
    }
}