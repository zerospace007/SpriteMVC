-- NormanYang
-- 2019年4月7日
-- message消息分发机制
local type = type
Message = {NumberMsg = {}, NormalMsg = {}}
local this = Message;
this.__index = this;

--添加Msg监听
function Message.AddMessage(msgName, func, listener)
	if not msgName or type(func) ~= 'function' then error('Add message Error!') end
	if type(msgName) == 'number' then
		this.NumberMsg[msgName] = func;
	elseif type(msgName) == 'string' then
		this.NormalMsg[msgName] = func;
		if listener then
			this.NormalMsg[msgName .. '_listener'] = listener;
		end
	end
end

--移除Msg监听
function Message.RemoveMessage(msgName)
	local msg = type(msgName) == 'number' and this.NumberMsg or this.NormalMsg;
	msg[msgName] = nil;
	this.NormalMsg[msgName .. '_listener'] = nil;
end

--广播Msg监听
function Message.DispatchMessage(msgName, ...)
	if type(msgName) == 'number' then
		local msg = this.NumberMsg;
		if msg[msgName] then
			local func = msg[msgName];
			spawn(func, ...);
		end
	elseif type(msgName) == 'string' then
		local msg = this.NormalMsg;
		local func = msg[msgName];
		if func then
			local listener = msg[msgName .. '_listener'];
			if listener then
				spawn(func, listener, ...);
			else
				spawn(func, ...);
			end
		end
	end
end