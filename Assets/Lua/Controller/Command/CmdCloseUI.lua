--region CmdClosePanel.lua
--Date 2016/02/17
--Author NormanYang
--关闭界面命令
--endregion

local Command = {};

function Command.Execute(notifyName, ...)
	local view, args = HandleNotifyParams(...);
	if not view then return end;
	view:Close(args);
end
return Command;