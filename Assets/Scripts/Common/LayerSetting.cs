using UnityEngine;

public class LayerSetting
{
    public const string Default = "Default";                            //默认显示层
    public const string UILayer = "UILayer";                           //界面显示层
    public const string MapLayer = "MapLayer";                          //地图显示层

    /// <summary>
    /// 改变层级
    /// </summary>
    /// <param name="gameObj">游戏显示对象</param>
    /// <param name="layerName">摄像机显示层级，默认为"Default"</param>
    public static void ChangeLayer(GameObject gameObj, string layerName = Default)
    {
        var transformArr = gameObj.GetComponentsInChildren<Transform>();
        foreach (var transform in transformArr)
        {
            transform.gameObject.layer = LayerMask.NameToLayer(layerName);
        }
    }

    /// <summary>
    /// 改变显示层级
    /// </summary>
    /// <param name="component"></param>
    /// <param name="layerName"></param>
    public static void ChangeLayer(Component component, string layerName = Default)
    {
        var gameObj = component.gameObject;
        ChangeLayer(gameObj, layerName);
    }
}