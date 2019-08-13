--region BaseViewList.lua
--Date 2019/04/22
--Author NormanYang
--基础显示列表
--endregion
require 'Framework/Utils/Message'
local remove = table.remove
local insert = table.insert
local tostring = tostring
local setmetatable = setmetatable

BaseViewList = {name = 'BaseViewList'}
local this = BaseViewList;
this.__index = this;

--创建基础显示列表对象--
function BaseViewList:New(name, itemClass, gridGroup, itemSrc)
	local oc = {name = name, itemClass = itemClass, gridGroup = gridGroup, itemSrc = itemSrc, viewList = {}};
	return setmetatable(oc, this);
end

--添加监听
function BaseViewList:AddMsgListener()
	local wrapName = self.wrapList.name;
	Message.AddMessage(wrapName .. BaseMessage.Insert, self.Insert, self);
	Message.AddMessage(wrapName .. BaseMessage.RemoveAt, self.RemoveAt, self);
	Message.AddMessage(wrapName .. BaseMessage.Update, self.Update, self);
	
	Message.AddMessage(wrapName .. BaseMessage.Clear, self.Clear, self);
	Message.AddMessage(wrapName .. BaseMessage.SetData, self.SetData, self);
	Message.AddMessage(wrapName .. BaseMessage.UpdateData, self.UpdateData, self);
end

--移除监听
function BaseViewList:RemoveMsgListener()
	if not self.wrapList or not self.wrapList.name then return end;
	local wrapName = self.wrapList.name;
	Message.RemoveMessage(wrapName .. BaseMessage.Insert);
	Message.RemoveMessage(wrapName .. BaseMessage.RemoveAt);
	Message.RemoveMessage(wrapName .. BaseMessage.Update);
	
	Message.RemoveMessage(wrapName .. BaseMessage.Clear);
	Message.RemoveMessage(wrapName .. BaseMessage.SetData);
	Message.RemoveMessage(wrapName .. BaseMessage.UpdateData);
end

--默认选中索引
function BaseViewList:SetSelectedIndex(index)
	if self.SelectedIndex <= 0 then
		self.SelectedIndex = index;
		local itemClass = self.viewList[1];
		if itemClass then itemClass:OnSelectItem();end
	end
	
	self.SelectedIndex = index;
	
	for node, itemClass in pairs(self.viewList) do
		itemClass:SetSelected(node == index);
	end
end

--选中索引
function BaseViewList:GetSelectedIndex()
	return self.SelectedIndex;
end

--获取选中显示列表项
function BaseViewList:GetSelectedItem()
	return self.viewList[self.SelectedIndex];
end

--添加一个显示列表项
function BaseViewList:Insert(index, dataItem)
	local count = #self.viewList;
	if index <= count then 
		self:CreateItem(index, dataItem);
		self:UpdateData();				--刷新显示整个列表
	else
		self:CreateItem(count + 1, dataItem);
	end
end

--移除一个显示列表项
function BaseViewList:RemoveAt(index)
	self:RemoveItem(index);
end

--更新显示一个显示列表项
function BaseViewList:Update(index, dataItem)
	self:UpdateItem(index, dataItem);
end

--清空显示列表项
function BaseViewList:Clear()
	local viewList = self.viewList;
	for index, itemClass in pairs(viewList) do
		if itemClass then itemClass:Destroy(); end
	end
	self.SelectedIndex = -1;
	self.wrapList = {};
	self.viewList = {};
end

--赋值列表项
function BaseViewList:SetData(wrapList)
	self:Clear();
	
	self.wrapList = wrapList;
	self:AddMsgListener();
	
	local dataList = wrapList.dataList;
	for index, dataItem in pairs(dataList) do
		self:CreateItem(index, dataItem);
	end
end

--创建显示列表项
function BaseViewList:CreateItem(index, dataItem)
	local name = self.name .. '_' .. 'Item@' .. tostring(index);
	local itemClass = self.itemClass:New(name);
	itemClass.gridGroup = self.gridGroup;
	itemClass.itemSrc = self.itemSrc;
	itemClass:SetData(index, dataItem, true);
	insert(self.viewList, index, itemClass);
end

--刷新显示列表项内容
function BaseViewList:UpdateItem(index, dataItem)
	local name = self.name .. '_' .. 'Item@' .. tostring(index);
	local itemClass = self.viewList[index];
	if not itemClass then return end;
	itemClass.name = name;
	itemClass.index = index;
	itemClass.dataItem = dataItem;
	itemClass:SetData(index, dataItem, false);
end

--移除显示列表项
function BaseViewList:RemoveItem(index)
	local itemClass = self.viewList[index];
	remove(self.viewList, index);
	if not itemClass then return end;
	itemClass:Destroy();
	
	self:UpdateData();					--刷新显示整个列表
end

--刷新显示整个列表
function BaseViewList:UpdateData()
	for node, itemClass in pairs(self.viewList) do
		local dataItem = self.wrapList.dataList[node];
		self:UpdateItem(node, dataItem);
	end
end

--释放View对象
function BaseViewList:Destroy()
	self:RemoveMsgListener();
	self:Clear();
	setmetatable(self, {});
end
return this;