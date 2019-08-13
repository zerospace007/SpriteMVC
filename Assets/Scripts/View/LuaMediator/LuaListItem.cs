using System.Collections.Generic;
using LuaInterface;
using UnityEngine;
using UnityEngine.UI;

public class LuaListItem : MonoBehaviour
{
    private Dictionary<string, LuaFunction> buttons = new Dictionary<string, LuaFunction>();
    /// <summary>
    /// 添加单击事件
    /// </summary>
    public void AddClick(GameObject gameObj, LuaFunction luaFunction, LuaTable self)
    {
        if (gameObj == null || luaFunction == null) return;

        AddClick(gameObj.GetComponent<Button>(), luaFunction, self);
    }

    /// <summary>
    /// 添加单击事件
    /// </summary>
    /// <param name="button"></param>
    /// <param name="luaFunction"></param>
    public void AddClick(Button button, LuaFunction luaFunction, LuaTable self)
    {
        if (button == null || luaFunction == null) return;
        buttons.Add(button.name, luaFunction);
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(delegate () { luaFunction.Call(self, button.gameObject); });
    }

    /// <summary>
    /// 添加Slider改变事件
    /// </summary>
    /// <param name="slider"></param>
    /// <param name="luaFunction"></param>
    public void AddSliderChange(Slider slider, LuaFunction luaFunction, LuaTable self)
    {
        if (null == slider || null == luaFunction) return;
        buttons.Add(slider.name, luaFunction);
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(delegate (float value) { luaFunction.Call(self, slider.gameObject, value); });
    }

    /// <summary>
    /// 添加分页项
    /// </summary>
    /// <param name="tog"></param>
    /// <param name="luaFunction"></param>
    /// <param name="self"></param>
    public void AddToggleClick(Toggle tog, LuaFunction luaFunction, LuaTable self)
    {
        if (tog == null || luaFunction == null) return;
        buttons.Add(tog.name, luaFunction);
        tog.onValueChanged.RemoveAllListeners();
        tog.onValueChanged.AddListener(delegate (bool bo) { luaFunction.Call(self, tog.gameObject, bo); });
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