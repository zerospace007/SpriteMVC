--region SvrListItem.lua
--Date 2019/04/23
--Author NormanYang
--基础显示列表项
--endregion
local setmetatable = setmetatable;

local SvrListItem = BaseListItem:New();
local this = SvrListItem;
this.__index = this;

SvrListItemMsgs = 
{
	SvrListSelectItem = 'SvrListSelectItem',
}

--创建基础显示列表项--
function SvrListItem:New(name)
	return setmetatable({name = name}, this);
end

--更新显示列表项内容（支持继承重写）
function SvrListItem:UpdateView()
	local TxtSvrName = self.transform:Find('TxtSvrName'):GetComponent(typeof(Text));
	local TxtSvrAdvise = self.transform:Find('TxtSvrAdvise').gameObject;
	TxtSvrAdvise:SetActive(self.dataItem.IsAdvise);
	TxtSvrName.text = self.dataItem.name;
end

--选中显示列表项（支持继承重写）
function SvrListItem:OnSelectItem(viewItem)
	SoundManager.Play(SoundName.Click);
	Message.DispatchMessage(SvrListItemMsgs.SvrListSelectItem, self.index, self.dataItem);
end

--释放View对象（支持继承重写）
function SvrListItem:Destroy()
	if self.gameObject then Destroy(self.gameObject) end;
	setmetatable(self, {});
end
return this;