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