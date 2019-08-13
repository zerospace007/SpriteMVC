-- NormanYang
-- 2019年03月18日

----错误码---------------------------------------
ErrorCode = 
{
	Error               = 0, 	-- 默认错误
	KeyWord_Faild       = 100, 	-- 登录关键字错误, 根据LoginType解释， 账号或密码错误/验证码错误
	Kick_By_OtherLogin 	= 101, 	-- 账号在其他位置登录
	Disconnect_Login	= 102,	-- 账号与登录服务器断开
	Player_NotFound		= 110, 	-- 非法的玩家连接
	Player_NickRepeat 	= 112,	-- 昵称重复
	World_IsMax		    = 120,	-- 世界已满，无法加入新的玩家
	Error_Parameter 	= 130,	-- 参数错误
	Error_Config 		= 131,	-- 配置错误
	Friend_Max		    = 140,	-- 对方好友数量已达上限
}

----服务器---------------------------------------
--服务器状态
local ServerStatus =
{
	Normal 		= 0,	-- 正常
	Maintain 	= 1,	-- 维护中，禁止再登录请求，并逐步踢出玩家
	Shutdown 	= 2,	-- 停机中
}
--服务器连接信息
local ServerInfo =
{
	sid		    = "",	-- 服务器编号
	name 		= "",	-- 服务器名称
	host		= "",	-- 连接地址
	port		= 0,	-- 连接端口
	status		= 0,	-- INT ServerStatus 服务器状态
	online		= 0,	-- INT 当前游戏人数
}
--登录类型
LoginType =
{
	None		= 0,	-- 游客
	Account 	= 1,	-- 账密
	Mobile		= 2,	-- 手机
	WeChat		= 3,	-- 微信
}
----账号-----------------------------------------
--账号级别类型
UserLevelType = 
{
	None  		= 0, 	-- 无类型
	Account		= 1,	-- 账号, keyword代表账号, data0表示密码
	Mobile		= 2,	-- 手机, keyword代表手机号, data0表示验证码, 根据流程第一次只发手机号, 第二次发手机号和验证码
	Acc_Mobile 	= 3,	-- 账号和手机, 已绑定2种方式的，可以用非游客任意形式登录
}
--账号 
local User =
{
	id		    = 0,	-- LONG 全服唯一ID
	loginCode	= "",	-- STRING 登录账号口令
	level 		= 0,	-- INT 账号级别 0 游客, 1 只绑定了账号, 2 只绑定了手机, 3 已绑定账号和手机  
	userinfo	= {},	-- UserInfo 账号信息
}
--游戏资料
local UserInfo =
{
	userId		= 0,	-- LONG User.id
	head		= "",	-- 头像（微信或其他或游戏自定义头像）
	nick		= "",	-- 昵称
	phone		= "",	-- 手机
}

----命令列表-------------------------------------
Protocal =
{
	--服务器连接相关
	Connect		= 100101,	--连接服务器
	Exception   = 100102,	--异常掉线
	Disconnect  = 100103,	--正常断线   
	Message		= 100104,	--接收消息
	
	--命令内容必须包含id属性，和命令等值，比如登录参数应该为：{ INT id: Protocal.Login, INT type: LoginType, STRING keyword: 关键字 }
	--服务器协议相关--
	ErrorCode 			= 0,			--通用命令 { INT id: 0, INT code: ErrorCode }
	
	--登录
	--C2S：{ INT type: LoginType, STRING keyword: 关键字, STRING data0: 备用0号, STRING data1: 备用1号, STRING data2: 备用2号 } 		
	--S2C：{ LONG userId: 用户ID }
	Login		 		= 100,
									
	--服务器列表
	--C2S: {}
	--S2C: { ServerInfo servers: [] }
	ServerList 			= 104,

	--服务器时间
    --C2S: {}
    --S2C: { long timestamp: 服务器当前截至时间 }
	ServerTime          = 108,

	-------------------------

	--用户数据
	--C2S: { LONG userId: 用户ID, LONG queryId: 待查询的用户ID }
	--S2C: { User user: 用户信息, BOOL create: 是否需要创建 }
	UserInfo			= 200,	

	--游戏数据--
	--C2S: { BOOL create: 创建/不创建, STRING nick: 角色昵称, INT sex: 性别 }
	GameData			= 202,
}