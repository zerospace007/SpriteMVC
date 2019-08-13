--region PopView.lua
--Date 2019/05/30
--Author NormanYang
--弹出界面
--endregion
local setmetatable = setmetatable
local wait = coroutine.wait
local start = coroutine.start

PopView = BaseView:New('PopView')
local this = PopView;
this.__index = this;
local TweenTime = 0.28;

local ScreenWidth = Screen.width;
local ScreenHeight = Screen.height;

--创建View对象--
function PopView:New(bundleName, atlasName)
	return setmetatable({bundleName = bundleName, atlasName = atlasName}, this);
end

--显示View对象
function PopView:Show(args)
	self.args = args;
	logWarn("Show Pop UI:--->>" .. self.bundleName);
	UIManager:ShowUI(self.bundleName, self.TweenShow, self);
	SoundManager.Play(SoundName.OpenUI);
	self.isLoaded = true;
	self.isShow = true;
end

function PopView:TweenShow(go)
	local transform = go.transform;
	transform.localPosition = Vector3.New(0, ScreenHeight, 0);
	transform.localScale = Vector3.zero;
	transform:DOLocalMove(Vector3.zero, TweenTime);
	transform:DOScale(Vector3.one, TweenTime):SetEase(Ease.InOutBack);
	self.transform = transform;
end

function PopView:TweenHide()
	local transform = self.transform;
	transform.localPosition = Vector3.zero;
	transform.localScale = Vector3.one;
	transform:DOLocalMove(Vector3.New(0, -ScreenWidth, 0), TweenTime);
	transform:DOScale(Vector3.zero, TweenTime):SetEase(Ease.InOutBack);
end

--隐藏View对象
function PopView:Hide()
	logWarn("Hide Pop UI:--->>" .. self.bundleName);
	if not self.isShow then return end
	SoundManager.Play(SoundName.Close);
	self:TweenHide();
	local function doHide()
		wait(TweenTime);
		UIManager:HideUI(self.bundleName);
		self.isShow = false;
	end
	start(doHide);
end

--销毁View对象
function PopView:Close()
	logWarn("Destroy Pop UI:--->>" .. self.bundleName);
	if not self.isShow then return end
	SoundManager.Play(SoundName.Close);
	self:TweenHide();
	local function doClose()
		wait(TweenTime);
		UIManager:CloseUI(self.bundleName);
		self.isLoaded = false;
	end
	start(doClose);
end
return this