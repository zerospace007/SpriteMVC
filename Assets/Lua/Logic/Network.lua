--region Network.lua
--Author : NormanYang
--Date   : 2019/03/09
--网络监听管理--
local LoginModel = require 'Model/LoginModel'	--登录模块
require 'Framework/Utils/Message'
json = require 'cjson'

Network = {};									--网络管理
local this = Network;
this.loginTimes = 0;							--第几次登录成功

--添加网络监听
function Network.Start() 
    log("Network.Start!!");
	Message.AddMessage(Protocal.Connect, this.OnConnect); 
	Message.AddMessage(Protocal.Message, this.OnMessage); 
	Message.AddMessage(Protocal.Exception, this.OnException); 
	Message.AddMessage(Protocal.Disconnect, this.OnDisconnect); 
	for k, v in pairs(ModelList) do 
		for k1, v1 in ipairs(v.MsgIdList) do
			if v1.msgid then Message.AddMessage(v1.msgid, v1.func) end
		end
	end
end

--卸载网络监听
function Network.Unload()
	Message.RemoveMessage(Protocal.Connect);
	Message.RemoveMessage(Protocal.Message);
	Message.RemoveMessage(Protocal.Exception);
	Message.RemoveMessage(Protocal.Disconnect);
    for k, v in pairs(ModelList) do      
        for k1, v1 in ipairs(v.MsgIdList) do
			if v1.msgid then Message.RemoveMessage(v1.msgid, v1.func) end
        end
    end
    logWarn('Unload Network...');
end

--Socket消息
function Network.OnSocket(key, data)
	Message.DispatchMessage(key, data);
end

--当连接建立时--
function Network.OnConnect()
	logWarn("Game Server connected!!");
	
	--针对登录和游戏服务器，第一次是登录服务器的返回，第二次是游戏服务器的返回
	if (this.loginTimes == 0) then
		logWarn('-------------登录成功')
		this.loginTimes = this.loginTimes + 1;
	elseif (this.loginTimes == 1) then
		logWarn('-------------登录游戏成功')
		local message = {};
		message.userid = LoginModel.User.userId;
		message.queryId = message.userid;
		LoginModel.C2SRoleInfo(message);
		this.loginTimes = 0;
	end
end

--异常断线--
function Network.OnException() 
	this.loginTimes = 0;
   	logError("OnException------->>>>");
	local content = LangManager.GetText('TxtException');
	local confirm = function()
		GameManager.ConnectSvr();
	end
	Facade.SendNotification(NotifyName.ShowUI, MsgBox, content, confirm);
end

--连接中断，或者被踢掉--
function Network.OnDisconnect() 
	this.loginTimes = 0;
    logError("OnDisconnect------->>>>");
	local content = LangManager.GetText('TxtDisconnect');
	local confirm = function()
		GameManager.ConnectSvr();
	end
	Facade.SendNotification(NotifyName.ShowUI, MsgBox, content, confirm);
end

--登录返回--
function Network.OnMessage(jsonStr) 
	local data = json.decode(jsonStr);
	logWarn('@--------Receive Network Message, Id>>: ' ..data.id .. ", data content>>: " .. jsonStr);
	this.OnSocket(data.id, data);--消息从新分发
end