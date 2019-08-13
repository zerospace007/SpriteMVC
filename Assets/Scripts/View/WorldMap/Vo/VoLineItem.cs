using UnityEngine;

public enum MarchingStatus
{
    None = 0,               //空闲，无队伍
    Marching_Go = 1,        //出征
    Marching_Work = 2,      //采集中
    Marching_Back = 3,      //行军返回
    Else = 4,	            //(客户端显示用)
}

/// <summary>
/// 路线信息
/// </summary>
public class VoLineItem
{
    public int index = 0;                               //行军索引，无用
    public MarchingStatus status = MarchingStatus.None; //行军状态
    public Vector2 userPos = Vector2.zero;              //出发点
    public Vector2 targetPos = Vector2.zero;            //目标点
    public bool isMine = true;                          //攻击者是否是自身
}