--Author : NormanYang
--Date   : 2019/03/09
--游戏管理器入口--
local start = coroutine.start
local isLoaded = coroutine.isLoaded
local Facade = require 'Framework/Facade'			--外观类（mvc框架入口）

require 'Controller/NotifyName'						--命令相关
require 'Common/Defines'							--游戏公共定义
require 'Common/Functions'							--公共方法
require 'Common/Protocal'							--网络协议定义
require	'Common/FrameDefines'						--框架公共定义（View和Model etc.）

require 'Logic/Network'								--网络管理
require 'Logic/DataManager'	 						--C#与lua数据传递etc
require 'Logic/TimeManager'							--时间管理
require 'Logic/SoundManager'						--声音音效管理
require 'Language/LangManager'						--语言包（可优化）

GameManager = {};									--游戏管理（游戏入口）
local this = GameManager;
local ip, port = '139.196.174.22', 13356;			--登录服务器地址与端口

--初始化FrameWork并初始化配置管理等
function GameManager.OnInit()
	this.InitFramework();							--初始化框架与命令
	Network.Start();								--初始化网络
	DataManager.Init();								--初始化载入配置列表或者游戏数据
	SoundManager.Init();							--初始化载入音乐音效
	this.OnInitOK();								--初始化完成，进入游戏
end

function GameManager.InitFramework()
	Facade.InitFramework();							--初始化框架--
													--注册命令--
	Facade.RegisterCommand(NotifyName.ShowUI, CmdList.CmdShowUI);
	Facade.RegisterCommand(NotifyName.HideUI, CmdList.CmdHideUI);
	Facade.RegisterCommand(NotifyName.CloseUI, CmdList.CmdCloseUI);
													--注册View通知列表--
	Facade.RegisterView(CameraHandler);
end

--初始化完成，发送链接服务器信息
function GameManager.OnInitOK()
	TipsManager:OnInit();												--初始化Tips
	SpriteAtlasManager.atlasRequested = SpriteAtlasManager.atlasRequested + this.SetAtlas;
	Facade.SendNotification(NotifyName.InitCameraHandler);				--初始化相机
	this.ConnectSvr();													--连接服务器
	log('Framework InitOK--->>>');
end

--显示登陆界面并连接服务器
function GameManager.ConnectSvr()
	Facade.SendNotification(NotifyName.ShowUI, LoginView);	--显示登录
	NetManager:SendConnect(ip, port);						--连接登录服务器
end

--赋值Atlas
function GameManager.SetAtlas(tag, action)
	log('Add view quote atlas resource: ' .. tag);
	local atlasLoader = BaseLoad:New(tag, BundleType.Atlas);
	atlasLoader:Load();
	
	local function loaded()
		isLoaded(atlasLoader);
		Util.DoAction(action, atlasLoader.loaded);
	end
	start(loaded);
end

--销毁--
function GameManager.OnDestroy()
	logWarn('Lua GameManager OnDestroy--->>>');
end
