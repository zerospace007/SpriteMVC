--公共方法
local create = coroutine.create
local resume = coroutine.resume
local concat = table.concat
local insert = table.insert
local remove = table.remove
local abs = math.abs
local len = string.len
local sub = string.sub
local srep = string.rep
local format = string.format
local type = type
local pairs = pairs
local tostring = tostring
local next = next

--协程执行函数
function spawn(func, ...)
	return resume(create(func), ...)
end

local Util = Util
--输出日志--
function log(str)
    Util.Log(str);
end

--输出日志--
function log(str, ...)
	Util.Log(format(str, ...));
end

--警告日志--
function logWarn(str) 
	Util.LogWarning(str);
end

--警告日志--
function logWarn(str, ...)
	Util.LogWarning(format(str, ...));
end

--错误日志--
function logError(str) 
	Util.LogError(str);
end

--Image置灰
function SetGrey(image, isGrey)
	UIManager:SetGrey(image, isGrey);
end

--transform归一化
function Identity(transform)
	transform.localPosition = Vector3.zero;
	transform.localScale = Vector3.one;
	transform.localRotation = Quaternion.identity;
end

--打印日志--
function Print(...)	
	local args = {...};
	local t = {}
    local str = ""
	for i=1, select('#',...) do
        local value = select(i, ...)
        str = str .. "\t" .. tostring(value)
        if type(value) == "table" then            
            str = str .. "\t" .. (table.getn(value) .. "\n")
            str = str .. (Print_r(value))
        end
	end
	Util.Log(str);
end

--查找对象--
function Find(gameObjName)
	return GameObject.Find(gameObjName);
end

--销毁游戏对象--
function Destroy(gameObj)
	GameObject.Destroy(gameObj);
end

--实例化游戏对象--
function Instantiate(prefab)
	return GameObject.Instantiate(prefab);
end

--列表移除
function List_Remove(list, attribute, value)
	for node=#list,1,-1 do
		local item = list[node];
		if item[attribute] == value then
			remove(list, node);
		end
	end
end

--列表添加，如果含有同类型列表，则更新覆盖
function List_Add(list, dataItem, attribute)
	local isHas, index = List_Has(list, dataItem, attribute);
	if isHas then list[index] = dataItem;
	else insert(list, 1, dataItem);end
end

--是否含有某列表项
function List_Has(list, dataItem, attribute)
	local isHas = false;local index = 1;
	for node, item in ipairs(list) do
		local value1 = tostring(item[attribute]);
		local value2 = tostring(dataItem[attribute]);
		if value1 == value2 then
			 isHas = true;index = node;
		end
	end
	return isHas, index;
end

--根据列名与属性值查找
function List_Find(list, attribute, value)
	if type(attribute) ~= 'string' then error('attribute type must be string') end
	for index, item in pairs (list) do
		if item[attribute] == value then
			return index, item;
		end
	end
	return 0, nil;
end
 
function Print_r(root)
    if not root then return "{}" end
	local cache = {  [root] = "." }
	local function _dump(t,space,name)
		local temp = {}
		for k,v in pairs(t) do
			local key = tostring(k)
			if cache[v] then
				insert(temp,"+" .. key .. " {" .. cache[v].."}")
			elseif type(v) == "table" then
				local new_key = name .. "." .. key
				cache[v] = new_key
				insert(temp,"\n".. "+" .. key .. _dump(v, space .. (next(t,k) and "|" or " " ).. srep(" ",(#name + #key)) , new_key))
			else
				insert(temp,"+" .. key .. " [" .. tostring(v).."]")
			end
		end
		return concat(temp,"\n\r"..space)
	end
    return _dump(root, "","")
end

--深复制Table--
function table.copy(ori_tab)  
    if (type(ori_tab) ~= "table") then  
        return nil  
    end  
    local new_tab = {}  
    for i,v in pairs(ori_tab) do  
        local vtyp = type(v)  
        if (vtyp == "table") then  
            new_tab[i] = table.copy(v)  
        elseif (vtyp == "thread") then  
            new_tab[i] = v  
        elseif (vtyp == "userdata") then  
            new_tab[i] = v  
        else  
            new_tab[i] = v  
        end  
    end  
    return new_tab  
end 

--分割字符串到table--
function string.split(str, delimiter)
    if str == nil or str == '' or delimiter == nil then
        return nil
    end
    
    local result = {}
    for match in (str..delimiter):gmatch("(.-)"..delimiter) do
        insert(result, match)
    end
    return result
end

--字体颜色选择
local ColorTable = 
{
	fcolor1 = 'E5ECEFFF',
	fcolor2 = '22E31FFF',
	fcolor3 = '1FD7E2FF',
	fcolor4 = '9E1FE2FF',
	fcolor5 = 'E29D1FFF',
	fcolor6 = 'E2481FFF',
	fcolor7 = 'E21FC1FF',
}

function GetColorStr(color, content)
	local color = ColorTable[color];
	content = format('<color="#'.. color ..'">%s</color>', content);
	return content;
end