using UnityEngine;

public class VoMapItem
{
    /// <summary>
    /// 激活地块
    /// </summary>
    public GameObject ObjMapItem;

    /// <summary>
    /// 建筑显示
    /// </summary>
    public GameObject ObjBuildItem;

    /// <summary>
    /// 地块X、Y轴坐标
    /// </summary>
    public Vector2 position;

    /// <summary>
    /// 是否越界
    /// </summary>
    public bool IsOutofBounds = false;

    /// <summary>
    /// 是否是深色地块
    /// </summary>
    public bool IsMapDeep = true;
    /// <summary>
    /// 地块Sprite
    /// </summary>
    public string MapSprite
    {
       get
        {
            var tempSprite = IsMapDeep ? "WorldMap1" : "WorldMap2";
            tempSprite = IsOutofBounds ? "WorldMap3" : tempSprite;
            return tempSprite;
        }
    }

    /// <summary>
    /// 城池
    /// </summary>
    public string CitySprite
    {
        get{ return null != WorldUnit.player ? "City1" : string.Empty; }
    }

    /// <summary>
    /// 资源
    /// </summary>
    public string ResSprite
    {
       get
        {
            switch (WorldUnit.res.resType)
            {
                case ResourcesType.Coin: return "BuildCoin";
                case ResourcesType.Coat: return "BuildCoat";
                case ResourcesType.Food: return "BuildFood";
                case ResourcesType.Metal: return "BuildMetal";
                case ResourcesType.Wood: return "BuildWood";
            }
            return string.Empty;
        }
    }

    /// <summary>
    /// 空地块（填充）
    /// </summary>
    public string NoneSprite
    {
        get
        {
            if (string.IsNullOrEmpty(m_NoneSprite))
            {
                int random = Random.Range(1, 5);
                m_NoneSprite = "WorldMap10" + random;
                return m_NoneSprite;
            }
            return m_NoneSprite;
        }
    }
    private string m_NoneSprite;

    public VoWorldUnit WorldUnit;   //地图单位信息
}
