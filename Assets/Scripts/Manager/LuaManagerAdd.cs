using LuaInterface;
using UnityEngine;

namespace Framework
{
    public partial class LuaManager : MonoBehaviour
    {
        /// <summary>
        /// 调用Lua 函数并传参（默认不超过8个参数）
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public R CallFunction<R>(string funcName, params object[] args)
        {
            if (null == mLuaState) return default;
            LuaFunction luaFunction = mLuaState.GetFunction(funcName);
            if (null == luaFunction) return default;
            var count = args.Length;
            switch (count)
            {
                case 1:
                    luaFunction.Invoke<object, R>(args[0]);
                    break;
                case 2:
                    luaFunction.Invoke<object, object, R>(args[0], args[1]);
                    break;
                case 3:
                    luaFunction.Invoke<object, object, object, R>(args[0], args[1], args[2]);
                    break;
                case 4:
                    luaFunction.Invoke<object, object, object, object, R>(args[0], args[1], args[2], args[3]);
                    break;

                case 5:
                    luaFunction.Invoke<object, object, object, object, object, R>(args[0], args[1], args[2], args[3], args[4]);
                    break;
                case 6:
                    luaFunction.Invoke<object, object, object, object, object, object, R>(args[0], args[1], args[2], args[3], args[4], args[5]);
                    break;
                case 7:
                    luaFunction.Invoke<object, object, object, object, object, object, object, R>(args[0], args[1], args[2], args[3], args[4], args[5], args[6]);
                    break;
                case 8:
                    luaFunction.Invoke<object, object, object, object, object, object, object, object, R>(args[0], args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
                    break;
                default:
                    luaFunction.Invoke<R>();
                    break;
            }

            return default;
        }
    }
}