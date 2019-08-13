local luaBehaviour;
local transform;
local gameObject;

--查看资源弹出框
MsgBox = TipsView:New('MsgBox', 'MsgBox');
local this = MsgBox;

--启动事件--
function MsgBox.Awake(obj)
	gameObject = obj;
	transform = obj.transform:Find('Content');
	luaBehaviour = gameObject:GetComponent(typeof(LuaBehaviour));
	
	local BtnDefine = transform:Find('BtnDefine').gameObject;
	local BtnCancel = transform:Find('BtnCancel').gameObject;
	this.BtnDefine = BtnDefine:GetComponent(typeof(RectTransform));
	this.TxtContent = transform:Find('Text'):GetComponent(typeof(Text));
	
	luaBehaviour:AddClick(BtnDefine, this.OnClick);
	luaBehaviour:AddClick(BtnCancel, this.OnClick);
end

function MsgBox.OnEnable()
	this.message = this.args[1];
	this.confirm = this.args[2];
	this.cancel = this.args[3];
	this.TxtContent.text = this.message;
	
end

function MsgBox.OnDisable()
end

local Switch =
{
	BtnDefine = function()
		this:Hide();
		if this.confirm then spawn(this.confirm) end
	end,
	BtnCancel = function()
		this:Hide();
		if this.cancel then spawn(this.cancel) end
	end,
}

--单击事件--
function MsgBox.OnClick(go)
	local func = Switch[go.name];
	if func then spawn(func) end
end

--单击事件--
function MsgBox.OnDestroy()
	logWarn("OnDestroy---->>>");
end

--通知列表--
this.NotifyList = 
{
}
return this;