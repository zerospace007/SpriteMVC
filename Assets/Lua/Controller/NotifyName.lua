--region NotificationName
--Date 2019.02/27
--Author NormanYang
--广播通知消息
--endregion

--处理消息参数
local select = select
local insert = table.insert

function HandleNotifyParams(...)
	local view, args = nil, {};
	local count = select('#', ...);
	for index = 1, count do
		if index == 1 then view = select(1, ...);
		else 
			local tempArg = select(index, ...);
			insert(args, index - 1, tempArg);
		end
	end
	return view, args;
end

--通知消息--
NotifyName = 
{
	
------------------------命令式通知---------------------------------------
	
	ShowUI = "ShowUI",								--显示界面--
	HideUI = "HideUI",								--隐藏界面--
	CloseUI = "CloseUI",							--关闭界面(执行销毁)--
	
------------------------界面通知---------------------------------------	
	
	--摄像机相关
	InitCameraHandler = 'InitCameraHandler',		--初始化摄像机控制
	SetSwitchType = 'SetSwitchType',				--设置摄像机切换类型（内城、世界等）
	SetPosition = 'SetPosition',					--设置中心点坐标
	SetCanMove = 'SetCanMove',						--是否可移动
	
	--角色相关
	PlayerInfoUpdate = 'PlayerInfoUpdate',			--玩家角色信息更新
	OtherPlayerInfoUpdate = 'OtherPlayerInfoUpdate',--其他玩家角色信息
}
	


CmdList = 
{
	--基础命令--
	CmdShowUI = require 'Controller/Command/CmdShowUI',
	CmdHideUI = require 'Controller/Command/CmdHideUI',
	CmdCloseUI = require 'Controller/Command/CmdCloseUI',
}