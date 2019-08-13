--region LoginModel.lua
--登录模块业务逻辑
--endregion
local LoginModel = BaseModel:New();
local this = LoginModel;

SexType = 				--性别类型
{ 
	None = 0,			--不详
	Man = 1,			--性别男
	Woman  = 2,			--性别女
}

this.User = {userId = 0, loginCode = 0, userinfo = {}};

--C2S：{ INT type: LoginType, STRING keyword: 关键字, STRING data0: 备用0号, STRING data1: 备用1号, STRING data2: 备用2号 } 				
function LoginModel.C2SLogin(message)
	DataManager.UserData.login = message;
	message.id = Protocal.Login;
	this.SendMessage(message);
end

--S2C：{ LONG userId: 用户ID }
function LoginModel.S2CLogin(data)
	this.User.userId = data.userId;
	DataManager.UserData.userId = data.userId;
	DataManager.SaveUserData();		--保存用户数据
end

--C2S: {}
function LoginModel.C2SServerList()
	local message = {};
	message.id = Protocal.ServerList;
	this.SendMessage(message);
end

--S2C: { ServerInfo servers: [] }
function LoginModel.S2CServerList(data)
	this.servers = data.servers;
	this.SendNotification(NotifyName.ShowUI, ChooseSvrView, this.servers);
end

--C2S: { LONG userId: 用户ID, LONG queryId: 待查询的用户ID }
function LoginModel.C2SRoleInfo(message)
	message.id = Protocal.UserInfo;
	this.SendMessage(message);
end

--S2C: { User user: 用户信息 }
function LoginModel.S2CUserInfo(data)
	if this.User.userId == data.user.id then		--自身用户数据
		this.User.userId = data.user.id;
		this.User.loginCode = data.user.loginCode;
		this.User.userinfo = data.user.userinfo;
		this.IsCreateRole = data.create;
		
		DataManager.UserData.User = this.User;
		DataManager.SaveUserData();					--保存用户数据
		this.CreateRoleOrNot();						--是否创建角色
	else											--其他用户数据
	end
end

--是否创建角色，创建角色则显示创角界面，否则直接进入游戏主界面
function LoginModel.CreateRoleOrNot()
	local message = {};
	message.create = this.IsCreateRole;
	if this.IsCreateRole then
		this.SendNotification(NotifyName.ShowUI, ChooseRoleView);
	else
		ShowTips('示例结束');return
		this.C2SGameData(message);
	end
end

--C2S: { BOOL create: 创建/不创建, STRING nick: 角色昵称, INT sex: 性别 }
function LoginModel.C2SGameData(message)
	message.id = Protocal.GameData;
	this.SendMessage(message);
	this.SendNotification(NotifyName.ShowUI, MainMenuView);		--显示主菜单
	this.SendNotification(NotifyName.ShowUI, BuildView);		--显示内城
end

--服务器时间
--C2S: {}
function LoginModel.C2SServerTime()
	this.SendMessage({id = Protocal.ServerTime});
end

--服务器时间
--S2C: { long timestamp: 服务器当前截至时间 }
function LoginModel.S2CServerTime(data)
	TimeManager.timestamp = data.timestamp / 1000;
	TimeManager.Start();
end

--网络协议，对应函数
this.MsgIdList = 
{
   	{msgid = Protocal.Login, func = this.S2CLogin},
	{msgid = Protocal.ServerList, func = this.S2CServerList},
	{msgid = Protocal.UserInfo, func = this.S2CUserInfo},
	{msgid = Protocal.ServerTime, func = this.S2CServerTime},
}
return this