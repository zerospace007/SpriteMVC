--region BaseLoad.lua
--Date 2019/03/30
--Author NormanYang
--基础加载类
--endregion
local setmetatable = setmetatable
local ResourceManager = ResourceManager

BundleType = 						--Bundle包类型（父目录）
{
	Base = 'base',					--基础包（无）
	UI = 'ui',						--UI类型
	Atlas = 'atlas',				--Atlas类型
	Soldiers = 'soldiers',			--士兵类型
	Audios = 'audios',				--声音音效
	Particle = 'particle',			--特效
	General = 'General',			--武将
}

BaseLoad = {bundleName = 'BaseLoad', bundleType = BundleType.Base}
local this = BaseLoad;
this.__index = this;
local loaders = {};					--加载管理

--创建Load对象
--参数说明：bundleName代表资源包名称，bundleType，代表资源类型
function BaseLoad:New(bundleName, bundleType)
	local loader = loaders[bundleName];
	if loader then return loader end;
	return setmetatable({bundleName = bundleName, bundleType = bundleType, isLoaded = false, loaded = {}}, this);
end

--Load加载方法，参数三种：
--1：无参数，代表加载自身，与bundleName相同的游戏资源
--2：string型参数，代表加载Bundle包内对应名称的游戏资源
--3：table型参数，代表加载字符串数组-Bundle包内对应名称的游戏资源
function BaseLoad:Load(resTable)
	if resTable then
		loaders[self.bundleName] = self;
		self.bundleFullName = self.GetBundle(self.bundleName, self.bundleType);
		if type(resTable) == 'string' then
			ResourceManager:LoadPrefab(self.bundleFullName, resTable, function(objs)
					self.loaded = objs[0];
					self.isLoaded = true;
				end)
		elseif type(resTable) == 'table' then
			local loadNum = #resTable;
			ResourceManager:LoadPrefab(self.bundleFullName, resTable, function(objs)
					for loadIndex = 1, loadNum do
						table.insert(self.loaded, objs[loadIndex - 1]);
					end
					self.isLoaded = true;
				end)
		end
	else
		resTable = self.bundleName;
		self:Load(resTable);
	end
end

--释放View对象
function BaseLoad:Destroy()
	if self.loaded then
		ResourceManager:UnloadAssetBundle(self.bundleFullName);
	end
	self.isLoaded = false;
	self.loaded = nil;
	loaders[self.bundleName] = nil;
	setmetatable(self, {});
end

--根据加载器名称获取Laod对象
function BaseLoad.GetLoader(loaderName)
	return loaders[loaderName];
end

--获取Bundle名称
function BaseLoad.GetBundle(name, bundleType)
	local bundleName = bundleType and (bundleType .. '/' .. name) or name;
	return bundleName;
end
return this;