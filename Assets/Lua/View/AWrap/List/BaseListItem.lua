--region BaseListItem.lua
--Date 2019/04/22
--Author NormanYang
--基础显示列表项
--endregion
local Facade = Facade
local setmetatable = setmetatable

BaseListItem = {};
local this = BaseListItem;
this.__index = this;

--创建基础显示列表项--
function BaseListItem:New(name)
	return setmetatable({name = name}, this);
end

--显示对象赋值
function BaseListItem:SetData(index, dataItem, isCreate)
	self.index = index;
	self.dataItem = dataItem;
	if isCreate then self:CreateView(); end
	self.transform:SetSiblingIndex(self.index);
	self.gameObject.name = self.name;
	self.itemBtn = self.gameObject:GetComponent(typeof(Button));
	
	self:UpdateView();
	self:RemoveListener();
	self:AddListener();
end

--创建显示列表项（不要继承这个function）
function BaseListItem:CreateView()
	self.gameObject = Instantiate(self.itemSrc);
	self.transform = self.gameObject.transform;
	local gameObject = self.gameObject;
	local transform = self.transform;
	self.luaListItem = gameObject:AddComponent(typeof(LuaListItem));
	
	if not gameObject.activeSelf then gameObject:SetActive(true); end
	transform:SetParent(self.gridGroup);
	transform.localPosition = Vector3.zero;
	transform.localScale = Vector3.one;
	transform.localRotation = Quaternion.identity;
end

function BaseListItem:SetSelected(isSelected)
	if self.selected then self.selected:SetActive(isSelected) end
end

--更新显示列表项内容（支持继承重写）
function BaseListItem:UpdateView()
end

--选中显示列表项（支持继承重写）
function BaseListItem:OnSelectItem(viewItem)
end

--添加监听（支持继承重写）
function BaseListItem:AddListener()
	if self.itemBtn then self.luaListItem:AddClick(self.itemBtn, self.OnSelectItem, self); end
end

--移除监听（支持继承重写）
function BaseListItem:RemoveListener()
	if self.itemBtn then self.luaListItem:RemoveClick(self.gameObject);end
end

--释放View对象（支持继承重写）
function BaseListItem:Destroy()
	if self.gameObject then Destroy(self.gameObject) end;
	setmetatable(self, {});
end

--发送通知--
function BaseListItem.SendNotification(notifyName, ...)
	Facade.SendNotification(notifyName, ...);
end
return this;