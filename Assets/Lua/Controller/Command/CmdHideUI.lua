--region CmdHidePanel.lua
--Date 2016/02/17
--Author NormanYang
--隐藏界面命令
--endregion

local Command = {};

function Command.Execute(notifyName, ...)
	local view, args = HandleNotifyParams(...);
	if not view then return end;
	view:Hide(args);
end
return Command;