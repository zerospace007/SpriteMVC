#region using
using System.Collections.Generic;
using Framework.Core;
using Framework.Utility;
using LuaInterface;
using UnityEngine;
using UnityEngine.UI;
#endregion

public class LuaBehaviour : View
{
    private Dictionary<string, LuaFunction> buttons = new Dictionary<string, LuaFunction>();

    protected void Awake()
    {
        Util.CallMethod(name, "Awake", gameObject);
    }

    protected void Start()
    {
        Util.CallMethod(name, "Start");
    }

    protected void OnEnable()
    {
        Util.CallMethod(name, "OnEnable");
    }

    protected void OnDisable()
    {
        Util.CallMethod(name, "OnDisable");
    }

    protected void OnDestroy()
    {
        Util.CallMethod(name, "OnDestroy");

        ClearClick();
        Util.ClearMemory();
        Debug.Log("~" + name + " was destroy!");
    }

    /// <summary>
    /// 添加单击事件
    /// </summary>
    public void AddClick(GameObject gameObj, LuaFunction luaFunction)
    {
        if (gameObj == null || luaFunction == null) return;

        AddClick(gameObj.GetComponent<Button>(), luaFunction);
    }

    public void AddToggleClick(GameObject gameObj, LuaFunction luaFunction)
    {
        if (gameObj == null || luaFunction == null) return;

        AddToggleClick(gameObj.GetComponent<Toggle>(), luaFunction);
    }

    /// <summary>
    /// 添加单击事件
    /// </summary>
    /// <param name="button"></param>
    /// <param name="luaFunction"></param>
    public void AddClick(Button button, LuaFunction luaFunction)
    {
        if (button == null || luaFunction == null) return;
        buttons.Add(button.name, luaFunction);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate () { luaFunction.Call(button.gameObject); });
    }

    public void AddToggleClick(Toggle tog, LuaFunction luaFunction)
    {
        if (tog == null || luaFunction == null) return;
        buttons.Add(tog.name, luaFunction);
        tog.onValueChanged.RemoveAllListeners();
        tog.onValueChanged.AddListener(delegate (bool bo) { luaFunction.Call(tog.gameObject, bo); });
    }

    /// <summary>
    /// 添加Slider改变事件
    /// </summary>
    /// <param name="slider"></param>
    /// <param name="luaFunction"></param>
    public void AddSliderChange(Slider slider, LuaFunction luaFunction)
    {
        if (null == slider || null == luaFunction) return;
        buttons.Add(slider.name, luaFunction);
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(delegate (float value) { luaFunction.Call(slider.gameObject, value); });
    }

    /// <summary>
    /// 添加分页列表项监听
    /// </summary>
    /// <param name="toggle"></param>
    /// <param name="luaFunction"></param>
    /// <param name="index"></param>
    /// <param name="itemData"></param>
    public void AddItemTogClick(Toggle toggle, LuaFunction luaFunction, int index, LuaTable itemData)
    {
        if (toggle == null || luaFunction == null) return;
        buttons.Add(toggle.name, luaFunction);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(delegate (bool val) { luaFunction.Call(toggle.gameObject, index, itemData, val); });
    }

    /// <summary>
    /// 删除单击事件
    /// </summary>
    /// <param name="gameObj"></param>
    public void RemoveClick(GameObject gameObj)
    {
        if (gameObj == null) return;
        if (buttons.TryGetValue(gameObj.name, out LuaFunction luafunc))
        {
            luafunc.Dispose();
            luafunc = null;
            buttons.Remove(gameObj.name);
        }
    }

    /// <summary>
    /// 清除单击事件
    /// </summary>
    public void ClearClick()
    {
        foreach (var button in buttons)
        {
            if (button.Value != null)
            {
                button.Value.Dispose();
            }
        }
        buttons.Clear();
    }

}