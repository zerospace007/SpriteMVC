local LoginModel = require 'Model/LoginModel'

local luaBehaviour;
local transform;
local gameObject;

--选择角色界面
ChooseRoleView = BaseView:New('ChooseRoleView');
local this = ChooseRoleView;

local VoRole = {sex = SexType.Man, nick = ''}						--创建角色信息
local PosInfo1 = {pos = {}, scale = {}};							--位置1的坐标与大小
local PosInfo2 = {pos = {}, scale = {}};							--位置2的坐标与大小

--启动事件--
function ChooseRoleView.Awake(obj)
	gameObject = obj;
	transform = obj.transform;
	luaBehaviour = transform:GetComponent(typeof(LuaBehaviour));
	this.randomName = ConfigAll.GetConfig(ConfigName.RandomName);
	
	this.InputRoleName = transform:Find('InputRoleName'):GetComponent(typeof(InputField));
	this.BtnRandom = transform:Find('BtnRandom'):GetComponent(typeof(Button));
	this.BtnWomen = transform:Find('BtnWomen'):GetComponent(typeof(Button));
	this.BtnMen = transform:Find('BtnMen'):GetComponent(typeof(Button));
	this.BtnStart = transform:Find('BtnStart'):GetComponent(typeof(Button));
	this.TxtDesc = transform:Find('TxtDesc'):GetComponent(typeof(Text));
	this.TxtDesc.text = LangManager.GetText('TxtUICreateRoleDesc');
	
	luaBehaviour:AddClick(this.BtnRandom, this.OnClick);
	luaBehaviour:AddClick(this.BtnWomen, this.OnClick);
	luaBehaviour:AddClick(this.BtnMen, this.OnClick);
	luaBehaviour:AddClick(this.BtnStart, this.OnClick);
	logWarn("Start lua--->>"..gameObject.name);
end

function ChooseRoleView.OnEnable()
	PosInfo1.pos = this.BtnMen.transform.localPosition;
	PosInfo1.scale = this.BtnMen.transform.localScale;
	
	PosInfo2.pos = this.BtnWomen.transform.localPosition;
	PosInfo2.scale = this.BtnWomen.transform.localScale;
	
	this.RandomNames();
end

function ChooseRoleView.OnDisable()
end

local Switch = 
{
	BtnRandom = function()
		this.RandomNames();
	end,
	BtnWomen = function()
		VoRole.sex = SexType.Woman;
		this.RandomNames();
		this.BtnWomen.transform:SetAsLastSibling();
		this.Tweening(PosInfo2, PosInfo1);
	end,
	BtnMen = function()
		VoRole.sex = SexType.Man;
		this.RandomNames();
		this.BtnMen.transform:SetAsLastSibling();
		this.Tweening(PosInfo1, PosInfo2);
	end,
	BtnStart = function()
		local roleName = this.InputRoleName.text;
		if string.len(roleName) > 0 then
			ShowTips('示例结束！');
		else ShowTips('请先输入角色名称！')
		end
	end,
}
--单击事件--
function ChooseRoleView.OnClick(go)
	log("OnClick---->>>"..go.name);
	SoundManager.Play(SoundName.Click);
	local func = Switch[go.name];
	if func then spawn(func) end;
	this.BtnWomen.interactable = VoRole.sex ~= SexType.Woman;
	this.BtnMen.interactable = VoRole.sex ~= SexType.Man;
end

--Tween动画
function ChooseRoleView.Tweening(pos1, pos2)
	this.BtnMen.transform:DOLocalMove(pos1.pos, 0.3);
	this.BtnMen.transform:DOScale(pos1.scale, 0.3);
	
	this.BtnWomen.transform:DOLocalMove(pos2.pos, 0.3);
	this.BtnWomen.transform:DOScale(pos2.scale, 0.3);
end

--随机名称
function ChooseRoleView.RandomNames()
	local length = #this.randomName;
	local indexs = {};
	math.randomseed(os.time());
	for i = 1, 3 do
		local index = math.random(length);
		table.insert(indexs, index);
	end
	
	local namePrefix = this.randomName[indexs[1]].adjective;
	local surnName = this.randomName[indexs[2]].fname;
	local isMale = VoRole.sex == SexType.Man;
	local name = isMale and this.randomName[indexs[3]].man or this.randomName[indexs[3]].woman;
	
	local fullName = namePrefix .. surnName .. name;
	this.InputRoleName.text = fullName;
	VoRole.nick = fullName;
end

--单击事件--
function ChooseRoleView.OnDestroy()
	logWarn("OnDestroy---->>>");
end

--通知列表--
this.NotifyList = 
{
}
return this;