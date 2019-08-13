--region BaseDataList.lua
--Date 2019/04/22
--Author NormanYang
--View 层基类
--endregion
local insert = table.insert
local remove = table.remove
local setmetatable = setmetatable 

BaseMessage =					--Base Message Name
{
	Insert = 'Insert',			--插入一个数据到Index位置
	RemoveAt = 'RemoveAt',		--移除Index位置的一个数据
	Update = 'Update',			--更新某条数据
	
	Clear = 'Clear',			--清除所有数据
	SetData = 'SetData',		--设置新的数据列表
	UpdateData = 'UpdateData',	--更新数据源列表
}

BaseDataList = {name = 'BaseDataList'}
local this = BaseDataList;
this.__index = this;

--创建基本数据--
function BaseDataList:New(name, dataList)
	dataList = dataList and dataList or {};
	return setmetatable({name = name, dataList = dataList}, this);
end

--设置新的DataList
function BaseDataList:SetData(dataList)
	if not dataList then 
		logWarn('dataList can not be nil');
		dataList = {};
	end
	self.dataList = dataList;
	local msgName = self.name .. BaseMessage.SetData;
	Message.DispatchMessage(msgName, self);
end

--不销毁重建，更新DataList
function BaseDataList:UpdateData(dataList)
	self.dataList = dataList;
	local msgName = self.name .. BaseMessage.UpdateData;
	Message.DispatchMessage(msgName, self);
end

--获取数据列表长度
function BaseDataList:GetCount()
	return #self.dataList;
end

--在index索引设置数据项
function BaseDataList:SetIndex(index, dataItem)
	if (self.dataList[index]) then
		self.dataList[index] = dataItem;
		local msgName = self.name .. BaseMessage.Update;
		Message.DispatchMessage(msgName, index, dataItem);
	else
		self:Insert(index, dataItem);
	end;
end

--获取index索引数据项
function BaseDataList:GetIndex(index)
	return self.dataList[index];
end

--添加一个数据项
function BaseDataList:Add(dataItem)
	insert(self.dataList, dataItem);
	local msgName = self.name .. BaseMessage.Insert;
	Message.DispatchMessage(msgName, self:GetCount() + 1, dataItem);
end

--插入一个数据项
function BaseDataList:Insert(index, dataItem)
	insert(self.dataList, index, dataItem)
	local msgName = self.name .. BaseMessage.Insert;
	Message.DispatchMessage(msgName, index, dataItem);
end

--移除一个数据项（根据某个属性字段值）
function BaseDataList:Remove(attribute, value)
	local index, item = self:Find(attribute, value);
	if index > 0 then self:RemoveAt(index) end
end

--查找一个数据项（根据某个属性字段值）
function BaseDataList:Find(attribute, value)
	if type(attribute) ~= 'string' then error('attribute type must be string') end
	for index, item in pairs (self.dataList) do
		if item[attribute] == value then
			return index, item;
		end
	end
	return 0, nil;
end

--移除一个数据项（根据索引值）
function BaseDataList:RemoveAt(index)
	remove(self.dataList, index)
	local msgName = self.name .. BaseMessage.RemoveAt;
	Message.DispatchMessage(msgName, index);
end

function BaseDataList:Clear()
	self.dataList = {};
	local msgName = self.name .. BaseMessage.Clear;
	Message.DispatchMessage(msgName);
end

function BaseDataList:Destroy()
	self:Clear();
	setmetatable(self, {});
end
return this;