local start = coroutine.start
local wait = coroutine.wait
local tonumber = tonumber

local luaBehaviour;
local transform;
local gameObject;

--查看资源弹出框
MsgTips = TipsView:New('MsgBox', 'MsgTips');
local this = MsgTips;
this.isTween = false;

--启动事件--
function MsgTips.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	luaBehaviour = gameObject:GetComponent(typeof(LuaBehaviour));
	
	this.ImageRect = transform:Find('Image'):GetComponent(typeof(RectTransform));
	this.TextRect = transform:Find('Text'):GetComponent(typeof(RectTransform));
	this.TxtContent = transform:Find('Text'):GetComponent(typeof(Text));
	luaBehaviour:AddClick(gameObject, this.OnClick);
end

function MsgTips.OnEnable()
	this.Content = this.args[1];
	this.TxtContent.text = this.Content;
	this.ImageRect.gameObject:SetActive(false);
	local function SetSizeDelta()
		wait(0.02);
		local rect = this.ImageRect.sizeDelta;
		rect.y = this.TextRect.sizeDelta.y + 40;
		this.ImageRect.sizeDelta = rect;
		this.ImageRect.gameObject:SetActive(true);
	end
	start(SetSizeDelta);
end

function MsgTips.OnDisable()
end

--单击事件--
function MsgTips.OnClick(go)
	this:Hide();
end

--单击事件--
function MsgTips.OnDestroy()
	logWarn("OnDestroy---->>>");
end

--通知列表--
this.NotifyList = 
{
}

return this;