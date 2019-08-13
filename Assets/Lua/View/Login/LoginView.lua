local LoginModel = require 'Model/LoginModel'

local len = string.len
local wait = coroutine.wait
local start = coroutine.start

local luaBehaviour;
local transform;
local gameObject;

--登录界面
LoginView = BaseView:New('LoginView');
local this = LoginView;

--启动事件--
function LoginView.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	luaBehaviour = transform:GetComponent(typeof(LuaBehaviour));
	
	this.PanelStart = transform:Find('PanelStart').gameObject;
	this.BtnStart = transform:Find('PanelStart/BtnStart'):GetComponent(typeof(Button));
	this.Toggle = transform:Find('PanelStart/Toggle'):GetComponent(typeof(Toggle));
	
	this.PanelLogin = transform:Find('PanelLogin').gameObject;
	this.InputFieldAccount = transform:Find('PanelLogin/InputFieldAccount'):GetComponent(typeof(InputField));
	this.InputFieldSecret = transform:Find('PanelLogin/InputFieldSecret'):GetComponent(typeof(InputField));
	this.BtnLogin = transform:Find('PanelLogin/BtnLogin'):GetComponent(typeof(Button));
	this.BtnRegister = transform:Find('PanelLogin/BtnRegister'):GetComponent(typeof(Button));
	this.BtnCancel = transform:Find('PanelLogin/BtnCancel'):GetComponent(typeof(Button));
	this.PanelLogin:SetActive(false);
	
	luaBehaviour:AddClick(this.BtnStart, this.OnClick);
	luaBehaviour:AddClick(this.BtnLogin, this.OnClick);
	luaBehaviour:AddClick(this.BtnRegister, this.OnClick);
	luaBehaviour:AddClick(this.BtnCancel, this.OnClick);
	logWarn("Start lua--->>"..gameObject.name);
end

function LoginView.OnEnable()
	this.Toggle.isOn = false;
	SoundManager.PlayBackSound(SoundName.BgmLogin);		--播放登录音乐
end

function LoginView.OnDisable()
end

local Switch = 
{
	BtnStart = function()
		SoundManager.Play(SoundName.Start);
		
		if this.Toggle.isOn then
			this.PanelStart:SetActive(false);
			this.PanelLogin:SetActive(true);
			this.PanelLogin.transform.localPosition = Vector3.New(0, -300, 0);
			this.PanelLogin.transform:DOLocalMoveY(0, 0.3);
			
			local login = DataManager.UserData.login;
			local isLogin = (login ~= nil);
			this.BtnLogin.gameObject:SetActive(isLogin);
			this.BtnRegister.gameObject:SetActive(not isLogin);
			if login then
				this.InputFieldAccount.text = login.keyword;
				this.InputFieldSecret.text = login.data0;
			end
		else
			ShowTips('请先确认玩家协议！');
		end
	end,
	BtnLogin = function()
		if len(this.InputFieldAccount.text) <= 0 or len(this.InputFieldSecret.text) <= 0 then
			ShowTips('账号或密码不能为空！'); 
			return;
		end
		if len(this.InputFieldSecret.text) < 6 then
			ShowTips('密码不能少于6个字符！'); 
			return;
		end
		local message = {};
		message.type = LoginType.Account;
		message.keyword = this.InputFieldAccount.text;
		message.data0 = this.InputFieldSecret.text;
		LoginModel.C2SLogin(message);
		
		this:Close();
	end,
	BtnRegister = function()
		if len(this.InputFieldAccount.text) <= 0 or len(this.InputFieldSecret.text) <= 0 then
			ShowTips('账号或密码不能为空！'); 
			return;
		end
		if len(this.InputFieldSecret.text) < 6 then
			ShowTips('密码不能少于6个字符！'); 
			return;
		end
		local message = {};
		message.type = LoginType.Account;
		message.keyword = this.InputFieldAccount.text;
		message.data0 = this.InputFieldSecret.text;
		LoginModel.C2SLogin(message);
		
		this:Close();
	end,
	BtnCancel = function()
		start(function ()
			wait(0.3);
			this.PanelStart:SetActive(true);
			this.PanelLogin:SetActive(false);
		end);
		this.PanelLogin.transform:DOLocalMoveY(-300, 0.3);
	end,
}

--单击事件--
function LoginView.OnClick(go)
	log("OnClick---->>>"..go.name);
	local func = Switch[go.name];
	if func then func() end;
end

--单击事件--
function LoginView.OnDestroy()
	logWarn("OnDestroy---->>>");
end

--通知列表--
this.NotifyList = 
{
}

return this;