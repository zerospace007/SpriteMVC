local LoginModel = require 'Model/LoginModel'
local SvrListItem = require 'View/ChooseSvr/SvrListItem'

local insert = table.insert
local remove = table.remove
local copy = table.copy

local luaBehaviour;
local transform;
local gameObject;

--选择服务器界面
ChooseSvrView = BaseView:New('ChooseSvrView');
local this = ChooseSvrView;

local SvrViewList;						--服务器显示列表
local SvrWrapList;						--服务器数据包装列表
local IsAdvise = true;					

--启动事件--
function ChooseSvrView.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	luaBehaviour = transform:GetComponent(typeof(LuaBehaviour));
	this.Toggle1 = transform:Find('PanelLeft/LeftChoose/Toggle1'):GetComponent(typeof(Toggle));
	this.Toggle2 = transform:Find('PanelLeft/LeftChoose/Toggle2'):GetComponent(typeof(Toggle));
	this.BtnBack = transform:Find('PanelTop/BtnBack'):GetComponent(typeof(Button));
	this.BtnLastest = transform:Find('PanelTop/TxtSvrName'):GetComponent(typeof(Button));
	this.TxtLastest = transform:Find('PanelTop/TxtSvrName'):GetComponent(typeof(Text));
	this.ImgState = transform:Find('PanelTop/ImgState'):GetComponent(typeof(Image));
	
	local grid = transform:Find('Scroll View/Viewport/Content');
	local item = grid:Find('SvrItem').gameObject;
	SvrWrapList = BaseDataList:New('SvrWrapList');
	SvrViewList = BaseViewList:New('SvrViewList', SvrListItem, grid, item);
	SvrViewList:SetData(SvrWrapList);
	
	luaBehaviour:AddClick(this.BtnBack, this.OnClick);
	luaBehaviour:AddClick(this.BtnLastest, this.OnClick);
	luaBehaviour:AddToggleClick(this.Toggle1, this.OnToggleClick);
	luaBehaviour:AddToggleClick(this.Toggle2, this.OnToggleClick);
end

function ChooseSvrView.OnEnable()
	this.servers = this.args[1];
	this.serversAdvise = {};
	
	--Test
	local serverInfo = this.servers[2];
	local serverInfo1 = copy(serverInfo);
	serverInfo.IsAdvise = false;
	serverInfo1.IsAdvise = true;
	for node = 1, 10 do
		if node % 2 == 0 then
			insert(this.serversAdvise, serverInfo1);
		end
		insert(this.servers, serverInfo);
	end
	
	this.Toggle2.isOn = false;
	IsAdvise = true;
	
	Message.AddMessage(SvrListItemMsgs.SvrListSelectItem, this.OnSelectItem);
	this.ShowLastestSvr();
	this.ShowSvrList();
end

function ChooseSvrView.OnDisable()
	Message.RemoveMessage(SvrListItemMsgs.SvrListSelectItem);
	
	if SvrViewList then SvrViewList:Clear(); end
	if SvrWrapList then SvrWrapList:Destroy(); end
	this.servers = {};
	this.serversAdvise = {};
end

function ChooseSvrView.ShowLastestSvr()
	local infoList = DataManager.UserData.InfoList;
	if not infoList then infoList = {} end;
	DataManager.UserData.InfoList = infoList;
	local infoLastest = infoList[1];
	if infoLastest then 
		this.Lastest = infoLastest;
		this.TxtLastest.text = infoLastest.ServerInfo.name;
	else
		this.TxtLastest.text = "";
	end
end

function ChooseSvrView.ShowSvrList()
	local tempDataList = IsAdvise and this.serversAdvise or this.servers;
	SvrWrapList:SetData(tempDataList);
end

--选中某个服务器
function ChooseSvrView.OnSelectItem(index, dataItem)
	local infoList = DataManager.UserData.InfoList;
	local num = #infoList;
	local function AddServer()		--添加一个服务器信息
		insert(infoList, 1, {ServerInfo = dataItem});
	end
	local function FindServer()		--查找一条服务器信息
		for node, voInfo in pairs(infoList) do
			local serverInfo = voInfo.ServerInfo;
			if dataItem.name == serverInfo.name then 
				return node, voInfo;
			end
		end
		return 0, nil;
	end
	if num > 0 then
		local node, voInfo = FindServer();
		if voInfo then
			if voInfo.PlayerInfo then
				remove(infoList, node);
				insert(infoList, 1, voInfo);
			else
				remove(infoList, node);
				AddServer();
			end
		else
			AddServer();
		end
	else
		AddServer();
	end
	
	DataManager.SaveUserData();						--保存用户数据
	
	log(dataItem.host .. "-----------" .. dataItem.port)
	NetManager:SendConnect(dataItem.host, dataItem.port);
	this:Close();
end

local Switch = 
{
	BtnBack = function()
		--this:Hide();
		--this.SendNotification(NotifyName.ShowUI, LoginView);				--显示登录
	end,
	TxtSvrName = function()
		if not this.Lastest then return end;
		local serverInfo = this.Lastest.ServerInfo;
		log(serverInfo.host .. "-----------" .. serverInfo.port)
		NetManager:SendConnect(serverInfo.host, serverInfo.port);
		
		this:Close();
	end
}

--单击事件--
function ChooseSvrView.OnClick(go)
	log("OnClick---->>>"..go.name);
	local func = Switch[go.name];
	if func then spawn(func) end
end

--选择分页
function ChooseSvrView.OnToggleClick(go, boolean)
	SoundManager.Play(SoundName.Click);
	if not boolean then return end;
	if go.name == 'Toggle1' then
		IsAdvise = true;
	elseif go.name == 'Toggle2' then
		IsAdvise = false;
	end
	this.ShowSvrList();
end

--销毁回调--
function ChooseSvrView.OnDestroy()
	logWarn("OnDestroy---->>>");
end

--通知列表--
this.NotifyList = 
{
}

return this;