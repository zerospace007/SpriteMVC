if GameConst.DebugMode then
	local _, LuaDebuggee = pcall(require, 'LuaDebuggee')
	if LuaDebuggee and LuaDebuggee.StartDebug then
		LuaDebuggee.StartDebug('127.0.0.1', 9826)
	else
		print('Please read the FAQ.pdf')
	end
end


local Util = Framework.Utility.Util;				--工具方法
local LuaHelper = Framework.Utility.LuaHelper;		--帮助
local LuaManager = LuaHelper.GetLuaManager();		--Lua连接管理

--lua bundle文件列表
local bundles = 
{
	--lua非显示逻辑
	'lua_common',
	'lua_config',
	'lua_controller',
	'lua_controller_command',
	'lua_framework',
	'lua_framework_utils',
	'lua_language',
	'lua_logic',
	'lua_model',
	
	--lua view显示逻辑
	'lua_view_awrap_list',					--数据与显示列表封装
	'lua_view_awrap_ui',					--显示封装
	
	'lua_view_camera',						--摄像机控制
	'lua_view_chooserole',					--选择角色
	'lua_view_choosesvr',					--选择服务器
	'lua_view_login',						--登录
	'lua_view_msgbox',						--弹出框
}

--添加lua bundle文件列表
function AddBundles()
	for node, bundleName in ipairs(bundles) do
		LuaManager:AddBundle(bundleName);
		Util.Log(string.format('@--Add bundle: %s', bundleName));
	end
	Util.Log('@--Add bundles sucess!');
end

--主入口函数。从这里开始lua逻辑
function Main()					
	Util.Log("@--logic start")
	if GameConst.LuaBundleMode then AddBundles() end;	
end

--场景切换通知
function OnLevelWasLoaded(level)
	collectgarbage('collect')
	Time.timeSinceLevelLoad = 0
end

function OnApplicationQuit()
end