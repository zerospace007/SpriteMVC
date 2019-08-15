# SpriteMVC
QQ群1：685243575

本框架工程基于Unity 2018.3.5 + UGUI + tolua构建

作为国内最早一批ulua和SimpleFrameWork的使用者与追随者，由衷感谢ulua/tolua以及LuaFramework的创建者与开发者为国产游戏热更新方案作出的贡献，同时对slua与xlua的作者表示真诚的感谢！（都是牛人，没有引战的意思）。

在使用LuaFramework的过程当中，为了项目的便利自己不断的修改整合，使用PureMVC的思维，用lua实现一套类似的逻辑，用于lua部分代码mvc的实现是修改的初衷，这便是框架的由来过程；C#大部分代码与逻辑仍沿用LuaFramework比如Assetbundle的下载以及加载，只有lua部分完全使用PureMVC方式另外实现，自己只能算是tolua、LuaFramework、PureMVC的整合搬运工。

网络协议数据序列化与反序列化部分直接使用的Json，其他Protobuf或二进制流方式可以参照tolua重新实现。

引用：

tolua#地址： https://github.com/topameng/tolua

LuaFrameWork地址：https://github.com/jarjin/LuaFramework_UGUI 

关于配置与语言包的生成与使用，是用Python 3使用xlrd库，生成lua配置文件，可读性较强，但是仍存有很多可优化的空间，只是本人对lua没有很深入的研究，暂时只想到如此处理办法：

xlsx2lua工具地址：https://github.com/zerospace007/xls2lua-python

语言包工具地址：https://github.com/zerospace007/xlsx2lua-language

关于AssetBundle打包工具，这里推荐:
https://github.com/Unity-Technologies/AssetBundles-Browser

最后在这里极力向大家推荐LuaPerfect编辑调试工具，由腾讯某技术组免费提供，支持断点调试，良心产品，友好支持ulua/tolua，slua，xlua。

https://github.com/jiangzheng1986/LuaPerfect

LuaPerfect官方群：932801740

##展示mvc核心代码，使用观察者模式，实现lua模块消息的发布-订阅
```lua
-- NormanYang
-- 2015年12月7日
-- 命令管理器
local setmetatable = setmetatable
local insert = table.insert

local Controller = {};
local this = Controller;
this.__index = this;

--命令管理类初始化--
function Controller.Init()
	log('Controller.Init----->>>');
    return setmetatable({commandList = {}, viewCommandList = {}}, this);
end

--执行命令--
function Controller:ExecuteCommand(notifyName, ...)
	local commandTable = self:GetCommand(notifyName);
	if commandTable ~= nil then
		commandTable.Execute(notifyName, ...);
	else
	 	local notifyList = {};
		for view, viewCommands in pairs(self.viewCommandList) do
			if viewCommands then
				for node, notifyExecute in ipairs(viewCommands) do
					if (notifyExecute.notifyName == notifyName) then
						insert(notifyList, {view = view, exeuteItem = notifyExecute});
					end
				end
			end
		end
		
 		if #notifyList <= 0 then return end
		for node, executeNode in ipairs(notifyList) do
			local exeuteItem = executeNode.exeuteItem;
			log("Exeute notification name>>: ".. exeuteItem.notifyName)
			local func = exeuteItem.func;
			spawn(func, ...);
		end
	end
end

--添加通知--
function Controller:RegisterCommand(commandName, command)
	self.commandList[commandName] = command;
end

--获取通知--
function Controller:GetCommand(commandName)
	 return self.commandList[commandName];
end

--移除通知--
function Controller:RemoveCommand(commandName)
	self.commandList[commandName] = {};
end

--注册view通知列表--
function Controller:RegisterView(view)
	if not view then return end
	self.viewCommandList[view.bundleName] = view.NotifyList;
end

--移除view通知列表--
function Controller:RemoveView(view)
	if not view then return end
	self.viewCommandList[view.bundleName] = nil;
end
return Controller;
```
