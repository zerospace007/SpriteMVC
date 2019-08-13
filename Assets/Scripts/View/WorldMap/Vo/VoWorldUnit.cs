using UnityEngine;

/// <summary>
/// 世界地块类型
/// </summary>
public enum MapType
{
    None = 0,		//无类型
    Resource = 1, 	//资源
    Player = 2, 	//玩家主城
}

public enum WorldFlag
{
    None = 0,       //无
    Self = 1,       //自己
    Friend = 2,     //友方
    Enemy = 3,      //敌方
}

/// <summary>
/// 资源类型
/// </summary>
public enum ResourcesType
{
    None = 0,       //无
    Food = 1,       //农田1
    Metal = 2,      //矿场2
    Coat = 3,       //狩猎场3
    Wood = 4,       //木材厂4
    Coin = 9,       //铸币厂9
}

/// <summary>
/// 地图单位
/// </summary>
public class VoWorldUnit
{
    public Vector2 pos;                 //Position 坐标
    public VoWorldResource res;         //资源对象
    public VoPlyer player;              //Resource 资源类型数据
    public MapType unitType;            //地块类型
    public WorldFlag flag;              //旗帜
}

public class VoPlyer
{
    public long id;             //Player id
    public string nickname; 	//玩家昵称
    public int level;           //等级
    public int fr_max;          //繁荣最大值
    public int fr_val;          //繁荣值
    public string legion;       //军团名称
}

/// <summary>
/// 世界资源
/// </summary>
public class VoWorldResource
{
    public string id;                       //Config.MapResourceRef.id 资源ID
    public int level;                       //资源等级
    public ResourcesType resType;           //资源类型ResourcesType
    public string troop;                    //Config.CampTroopsRef.id 驻军阵型ID
}