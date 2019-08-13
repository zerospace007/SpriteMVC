--region BaseView.lua
--Date 2019/03/30
--Author NormanYang
--View 层基类
--endregion
local setmetatable = setmetatable
local wait = coroutine.wait

BaseView = {}
local this = BaseView;
this.__index = this;

--创建View对象--
function BaseView:New(bundleName, atlasName)
    return setmetatable({bundleName = bundleName, atlasName = atlasName, isShow = false, isLoaded = false}, this);
end

--显示View对象
function BaseView:Show(args)
	self.args = args;
	logWarn("Show Object:--->>" .. self.bundleName);
	UIManager:ShowUI(self.bundleName);
	self.isLoaded = true;
	self.isShow = true;
end

--隐藏View对象
function BaseView:Hide()
	logWarn("Hide Object:--->>" .. self.bundleName);
	UIManager:HideUI(self.bundleName);
	self.isShow = false;
end

--销毁View对象
function BaseView:Close()
	logWarn("Destroy Object:--->>" .. self.bundleName);
	UIManager:CloseUI(self.bundleName);
	self.isLoaded = false;
end

--获取界面Atlas
function BaseView:SetAtlas(func)
	local GetLoader = BaseLoad.GetLoader;
	self.viewAtlas = GetLoader(self.atlasName);
	if (not self.viewAtlas) then
		wait(0.1);
		self.viewAtlas = GetLoader(self.atlasName);
	end
	if (func) then spawn(func) end;
	return self.viewAtlas; 
end

--释放View对象
function BaseView:Destroy()
    setmetatable(self, {});
end

--发送通知--
function BaseView.SendNotification(notifyName, ...)
	Facade.SendNotification(notifyName, ...);
end

--显示弹出框
function ShowMsgBox(content, confirm, cancel)
	this.SendNotification(NotifyName.ShowUI, MsgBox, content, confirm, cancel);
end

--显示弹出tips
function ShowMsgTips(content)
	this.SendNotification(NotifyName.ShowUI, MsgTips, content);
end
return this