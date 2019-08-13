/*
Copyright (c) 2015-2017 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using LuaInterface;
using UnityEngine;

/// <summary>
/// lua 循环事件
/// </summary>
public class LuaLooper : MonoBehaviour
{
    public LuaBeatEvent UpdateEvent { get; private set; }

    public LuaBeatEvent LateUpdateEvent { get; private set; }

    public LuaBeatEvent FixedUpdateEvent { get; private set; }

    public LuaState mLuaState = null;

    private void Start()
    {
        try
        {
            UpdateEvent = GetEvent("UpdateBeat");
            LateUpdateEvent = GetEvent("LateUpdateBeat");
            FixedUpdateEvent = GetEvent("FixedUpdateBeat");
        }
        catch (Exception exception)
        {
            Destroy(this);
            throw exception;
        }
    }

    /// <summary>
    /// 获取lua事件
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private LuaBeatEvent GetEvent(string name)
    {
        LuaTable table = mLuaState.GetTable(name);

        if (table == null)
        {
            throw new LuaException(string.Format("Lua table {0} not exists", name));
        }

        LuaBeatEvent beatEvent = new LuaBeatEvent(table);
        table.Dispose();
        table = null;
        return beatEvent;
    }

    private void ThrowException()
    {
        string error = mLuaState.LuaToString(-1);
        mLuaState.LuaPop(2);
        throw new LuaException(error, LuaException.GetLastError());
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (mLuaState == null) return;
#endif
        if (mLuaState.LuaUpdate(Time.deltaTime, Time.unscaledDeltaTime) != 0)
        {
            ThrowException();
        }

        mLuaState.LuaPop(1);
        mLuaState.Collect();
#if UNITY_EDITOR
        mLuaState.CheckTop();
#endif
    }

    private void LateUpdate()
    {
#if UNITY_EDITOR
        if (mLuaState == null) return;
#endif
        if (mLuaState.LuaLateUpdate() != 0)
        {
            ThrowException();
        }

        mLuaState.LuaPop(1);
    }

    private void FixedUpdate()
    {
#if UNITY_EDITOR
        if (mLuaState == null) return;
#endif
        if (mLuaState.LuaFixedUpdate(Time.fixedDeltaTime) != 0)
        {
            ThrowException();
        }

        mLuaState.LuaPop(1);
    }

    public void Destroy()
    {
        if (mLuaState != null)
        {
            if (UpdateEvent != null)
            {
                UpdateEvent.Dispose();
                UpdateEvent = null;
            }

            if (LateUpdateEvent != null)
            {
                LateUpdateEvent.Dispose();
                LateUpdateEvent = null;
            }

            if (FixedUpdateEvent != null)
            {
                FixedUpdateEvent.Dispose();
                FixedUpdateEvent = null;
            }

            mLuaState = null;
        }
    }

    private void OnDestroy()
    {
        if (mLuaState != null)
        {
            Destroy();
        }
    }
}
