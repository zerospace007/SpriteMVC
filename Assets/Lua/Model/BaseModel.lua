--region BaseModel.lua
--业务逻辑基类
--endregion
local setmetatable = setmetatable;
BaseModel = {};
local this = BaseModel;
this.__index = this;

--创建Model对象
function BaseModel:New()
    return setmetatable({}, this);
end

--发送网络消息--
function BaseModel.SendMessage(message)
	local jsonStr = json.encode(message);
	logWarn('@--------Send Network Message, Id>>: ' .. message.id .. ', data content>>: ' .. jsonStr);
	NetManager:SendNetMessage(jsonStr);
end

--发送通知--
function BaseModel.SendNotification(notifyName, ...)
	Facade.SendNotification(notifyName, ...);
end

--释放Model对象--
function BaseModel:Destroy()
    setmetatable(self, {});
end
return this;