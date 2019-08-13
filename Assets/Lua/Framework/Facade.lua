--region Facade.lua
--Date 2016/02/2
--Author NormanYang
--外观管理类
--endregion

require 'Controller/NotifyName'
local Controller = require 'Framework/Controller'

Facade = {};
local controller;			--消息管理--

--初始化外观，注册通知消息--
function Facade.InitFramework()
	log("Facade Init Framework");
    controller = Controller.Init();
end

--注册通知到指定的命令--
function Facade.RegisterCommand(commandName, commandTable)
	controller:RegisterCommand(commandName, commandTable);
end

--移除消息--
function Facade.RemoveCommand(commandName)
	controller:RemoveCommand(commandName);
end

--注册View通知列表--
function Facade.RegisterView(view)
	controller:RegisterView(view);
end

--移除View通知列表--
function Facade.RemoveView(view)
	controller:RemoveView(view);
end

--发送消息体并执行--
function Facade.SendNotification(notifyName, ...)
	controller:ExecuteCommand(notifyName, ...);
end
return Facade;